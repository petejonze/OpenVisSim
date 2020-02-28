using System;
using UnityEngine;

namespace VisSim
{
    
    public class myInpainter : LinkableBaseEffect
    {
        [Linkable, Range(1, 7), TweakableMember(1, 7, "N Iterations", "Filling In")]
        public int N_Iterations = 3;
        private int _old_N_Iterations = 1;

        public bool useFieldTexture = false;
        private myFieldLoss fieldLossHandle;
        public Texture2D scotomaTexture;
        [Linkable, Range(0f, 1f)]
        public float threshold = 0.5f; // values less-than-or-equal to this will be in-painted
        private float _old_threshold = 0.5f;

        // internal
        private Color[] px;
        private float maxDist = 0f;
        private float[] dist;
        private float[] angle;

        // Use this for initialization
        public new void OnEnable()
        {
            base.OnEnable();

            // load texture (in future, could be generated at runtime, as per warp, etc.)
            fieldLossHandle = this.gameObject.GetComponent<myFieldLoss>();
            if (useFieldTexture)
            {
                scotomaTexture = fieldLossHandle.overlayTexture;
            } else {
                scotomaTexture = (Texture2D)Resources.Load("macular-degeneration", typeof(Texture2D)); // blindpatches circle_512 blob2d_1024  gradiant1d_1024
            }
            this.generateFromTexture(scotomaTexture);
        }
        
        public void generateFromTexture(Texture2D scotomaTexture)
        {
            if (scotomaTexture == null)
            {
                Debug.Log("Error: file empty or null\n");
                // TODO - CRASH THE SCRIPT?
            }

            // count number of visible (white) pixels
            int count = 0;
            px = scotomaTexture.GetPixels();
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].grayscale > threshold)
                {
                    count++;
                }
            }
            //Debug.LogFormat("{0} above-criterion (visible) values detected, out of {1} values total", count, px.Length);

            // check any (and not all) values in need of inpainting
            if ((count==0) || (count == px.Length))
            {
                Debug.LogFormat("{0} above-criterion (visible) values detected, out of {1} values total. Effect will be disabled!", count, px.Length);
                this.enabled = false;
                return;
            }

            // feed visible pixels into Alglib's nearest neighbour (kd-tree) search utility
            double[,] visibleLocs = new double[count, 2];
            int ii = 0;
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].grayscale > threshold)
                {
                    visibleLocs[ii, 0] = i % scotomaTexture.width; // x
                    visibleLocs[ii, 1] = (int)(i / scotomaTexture.width); // y
                    ii++;
                }
            }
            //Debug.Log(">>>> " + ii);

            // build kd-tree
            int nx = 2;
            int ny = 0;
            int normtype = 2;
            alglib.kdtree kdt;
            double[,] r = new double[0, 0];
            alglib.kdtreebuild(visibleLocs, nx, ny, normtype, out kdt);

            // for each non-visible (white) pixel, find the nearest pixel coordinate and compute distance
            // keep running tally of furthest distance
            float xOffset;
            float yOffset;
            dist = new float[scotomaTexture.width * scotomaTexture.height];
            angle = new float[scotomaTexture.width * scotomaTexture.height];
            int x = 0;
            int y = 0;
            double[] X;
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].grayscale <= threshold)
                {
                    x = i % scotomaTexture.width;
                    y = (int)(i / scotomaTexture.width);


                    // find nearest neighbour
                    X = new double[] { x, y };
                    alglib.kdtreequeryknn(kdt, X, 1);
                    alglib.kdtreequeryresultsx(kdt, ref r); // read answer out into "r"


                   

                    // Cartesian to Polar:
                    // compute offset (sign only) and distance
                    try
                    {
                        xOffset = x - (float)r[0, 0];
                    }
                    catch (Exception e)
                    {
                        Debug.Log(x + ", " + y + ": " + r.Length);
                        throw;
                    }
                    yOffset = (float)r[0, 1] - y;
                    dist[i] = Mathf.Sqrt(Mathf.Pow(xOffset, 2) + Mathf.Pow(yOffset, 2));
                    angle[i] = Mathf.Atan2(yOffset, xOffset);

                    // keep running tally of max distance
                    maxDist = Mathf.Max(maxDist, dist[i]);
                }
            }
            //Debug.LogFormat("Max distance is {0} pixels", maxDist);

            // Compute!
            compute();


            // return focus to GameView to ensure mouse keeps reporting coordinates
            #if UNITY_EDITOR
                        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                        UnityEditor.SceneView.FocusWindowIfItsOpen(T);
            #endif
        }

        private void compute()
        {
            // compute offsets (0 to 1)
            float nPixelsPerIteration = maxDist / N_Iterations;
            int[,] iterationN = new int[scotomaTexture.width, scotomaTexture.height];
            Color[] xOffsetColor = new Color[scotomaTexture.width * scotomaTexture.height];
            Color[] yOffsetColor = new Color[scotomaTexture.width * scotomaTexture.height];
            float offsetMagnitude, xOffset, yOffset;
            int x, y;
            //Debug.LogFormat("nPixelsPerIteration = {0}", nPixelsPerIteration);
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].grayscale <= threshold)
                {
                    x = i % scotomaTexture.width;
                    y = (int)(i / scotomaTexture.width);

                    iterationN[x, y] = (int)Mathf.Ceil(dist[i] / nPixelsPerIteration);

                    // convert to -1 to 1;
                    offsetMagnitude = Mathf.Ceil(iterationN[x, y] * nPixelsPerIteration);
                    //offsetMagnitude *= Mathf.Sqrt(2) * iterationN[x, y];
                    //offsetMagnitude = dist[i] * iterationN[x, y] * nPixelsPerIteration);
                    offsetMagnitude *= Mathf.Abs(angle[i] % (Mathf.PI / 2)) * 0.5f + 1; // Distance extra to stop overlap (necessary?)
                    // Polar to Cartesian:
                    xOffset = (offsetMagnitude / Screen.width) * Mathf.Cos(angle[i]);
                    yOffset = (offsetMagnitude / Screen.height) * Mathf.Sin(angle[i]);

                    // convert to 0 to 1
                    xOffset = (xOffset + 1) / 2;
                    yOffset = (yOffset + 1) / 2;

                    // convert to color
                    xOffsetColor[i] = new Color(xOffset, xOffset, xOffset, 1f);
                    yOffsetColor[i] = new Color(yOffset, yOffset, yOffset, 1f);
                }
                else
                {
                    xOffsetColor[i] = new Color(0.5f, 0.5f, 0.5f, 1f);
                    yOffsetColor[i] = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }

            // calculate edges for smoothing/blurring
            float edgethreshold = 0.666f;
            Color[] blurMatrix = new Color[scotomaTexture.width * scotomaTexture.height];
            for (x = 1; x < (scotomaTexture.width - 1); x++)
            {
                for (y = 1; y < (scotomaTexture.height - 1); y++)
                {
                    float x0 = angle[x + y * scotomaTexture.height];
                    float x1 = angle[(x - 1) + y * scotomaTexture.height];
                    float x2 = angle[(x + 1) + y * scotomaTexture.height];
                    float x3 = angle[x + (y - 1) * scotomaTexture.height];
                    float x4 = angle[x + (y + 1) * scotomaTexture.height];

                    if (Mathf.Abs(x0 - x1) > edgethreshold || Mathf.Abs(x0 - x2) > edgethreshold || Mathf.Abs(x0 - x3) > edgethreshold || Mathf.Abs(x0 - x4) > edgethreshold)
                    {
                        blurMatrix[x + y * scotomaTexture.height] = Color.white;
                    }
                    else
                    {
                        blurMatrix[x + y * scotomaTexture.height] = Color.black;
                    }
                }
            }

            // Convert matrixes to textures
            Texture2D offsetTextureX = new Texture2D(scotomaTexture.width, scotomaTexture.height, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            offsetTextureX.SetPixels(xOffsetColor);
            offsetTextureX.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
            Texture2D offsetTextureY = new Texture2D(scotomaTexture.width, scotomaTexture.height, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            offsetTextureY.SetPixels(yOffsetColor);
            offsetTextureY.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
            Texture2D blurTexture = new Texture2D(scotomaTexture.width, scotomaTexture.height, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            blurTexture.SetPixels(blurMatrix);
            blurTexture.Apply(false); // actually apply all SetPixels, don't recalculate mip levels

            // for debugging: draw texture to screen!
            //GameObject currentLoc = GameObject.Find("Cube (Street Scene)"); //("Cube (Texture2D)");
            //currentLoc.GetComponent<Renderer>().Material.mainTexture = blurTexture; // offsetTextureX;

            // set texture as overlays in shader
            Material.SetTexture("_OffsetTextureX", offsetTextureX);
            Material.SetTexture("_OffsetTextureY", offsetTextureY);
            Material.SetTexture("_BlurTexture", blurTexture);
        }        

        // Update is called once per frame
		protected override void OnUpdate()
        {
            // XXXX
            if (useFieldTexture)
            {
                if (scotomaTexture != fieldLossHandle.overlayTexture)
                {
                    scotomaTexture = fieldLossHandle.overlayTexture;
                    this.generateFromTexture(scotomaTexture);
                }
            }

            // Gaze-contingent
            Vector2 xy_norm = GazeTracker.GetInstance.xy_norm;
            Material.SetFloat("_MouseX", 1 - xy_norm.x); // Why 1-x???
            Material.SetFloat("_MouseY", xy_norm.y);
            
            // update params
            if ((N_Iterations != _old_N_Iterations) || (threshold != _old_threshold))
            {
                compute();
                _old_N_Iterations = N_Iterations;
                _old_threshold = threshold;
            }
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // simple mode
            //Graphics.Blit(source, destination, Material, 0);

            // complex mode (usgin blurring)
            RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture tmp1 = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, tmp, Material, 0);
            Graphics.Blit(tmp, tmp1, Material, 1);
            Graphics.Blit(tmp1, destination, Material, 2);
            RenderTexture.active = null; // ???
            RenderTexture.ReleaseTemporary(tmp);
            RenderTexture.ReleaseTemporary(tmp1);
        }
		
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myInpainter";
		}
    }
}
