// TODO -- have a transparency option for blending inpainted region with original image?
using UnityEngine;

namespace VisSim
{
    
    public class myInpainter2_hacked : LinkableBaseEffect
    {
        [Linkable]
        public bool useFieldTexture = false;
        [Linkable]
        public Texture2D scotomaTexture;
        [Linkable, Range(0f, 1f)]
        public float threshold = 0.5f; // values less-than-or-equal to this will be inpainted
        [Linkable]
        public Texture2D inpainterTexture;

        // internal: for checking for parameter changes
        private Texture2D _old_scotomaTexture;
        private float _old_threshold = 0.5f;

        // internal: for generating inpainter overlay
        private myFieldLoss fieldLossHandle;
        private Color[] maskImgMatrix;
        private Color[] cardinalOffsetsImgMatrix;
        private Texture2D cardinalOffsetsTexture;
        //private Texture2D diagonalOffsetsTexture; <-- not currently implemented
        private bool[,] isMask;

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

            // create new texture by thresholding existing texture
            // TODO -- speed up by doing on the GPU?
            var temp = Time.realtimeSinceStartup;
            computeMask();
            print("Time for computeMask: " + (Time.realtimeSinceStartup - temp).ToString("f6"));

            // XXXXXXXX
            // TODO -- speed up by doing on the GPU?
            temp = Time.realtimeSinceStartup;
            compute();
            print("Time for compute: " + (Time.realtimeSinceStartup - temp).ToString("f6"));

            // XXXXXXXX
            _old_scotomaTexture = scotomaTexture;
            _old_threshold = threshold;

            // XXXXXXXX
            Material.SetTexture("_Overlay", cardinalOffsetsTexture);

            // return focus to GameView to ensure mouse keeps reporting coordinates
            #if UNITY_EDITOR
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                UnityEditor.SceneView.FocusWindowIfItsOpen(T);
            #endif
        }

        // Experimental:
        public bool useComputeShader = true;
        public ComputeShader shader;
        private RenderTexture _inpainterTexture;
        public ComputeShader shader2;
        //public Texture2D inpainterTexture2;

        

        private void computeMask()
        {
            if (useComputeShader && SystemInfo.supportsComputeShaders)
            {
                Debug.Log("Compute shader is supported");

                // make a low-res copy to work from
                Texture2D _scotomaTexture = Instantiate(scotomaTexture);
                TextureScale.Bilinear(_scotomaTexture, 256, 256);

                // = (ComputeShader)Resources.Load("MyFirstComputer");
                int kernelIndex = shader.FindKernel("CSMain");

                // generate texture
                // TODO -- only do this if/when scotomaTexture changes
                _inpainterTexture = new RenderTexture(_scotomaTexture.width, _scotomaTexture.height, 32); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
                _inpainterTexture.enableRandomWrite = true;
                _inpainterTexture.Create();
                Graphics.Blit(scotomaTexture, _inpainterTexture);

                shader.SetFloat("Threshold", threshold);
                shader.SetTexture(kernelIndex, "Result", _inpainterTexture);
                shader.Dispatch(kernelIndex, _scotomaTexture.width, _scotomaTexture.height, 1);

                inpainterTexture = new Texture2D(_inpainterTexture.width, _inpainterTexture.height, TextureFormat.ARGB32, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
                RenderTexture.active = _inpainterTexture;
                inpainterTexture.ReadPixels(new Rect(0, 0, _inpainterTexture.width, _inpainterTexture.height), 0, 0);
                inpainterTexture.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
            } else
            {
                //Debug.Log("Compute shader is NOT supported");

                // make a low-res copy to work from
                Texture2D _scotomaTexture = Instantiate(scotomaTexture);
                TextureScale.Bilinear(_scotomaTexture, 256, 256);

                // generate texture
                Color[] px_in = _scotomaTexture.GetPixels();
                inpainterTexture = new Texture2D(_scotomaTexture.width, _scotomaTexture.height, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
                maskImgMatrix = new Color[_scotomaTexture.width * _scotomaTexture.height];
                isMask = new bool[_scotomaTexture.width, _scotomaTexture.height];
                for (int x = 0; x < _scotomaTexture.width; x++)
                {
                    for (int y = 0; y < _scotomaTexture.height; y++)
                    {
                        if  (x == 0 || x == (_scotomaTexture.width - 1) || y == 0 || y == (_scotomaTexture.height - 1))
                        {
                            // edges (!) [TMP HACK]
                            maskImgMatrix[x + y * _scotomaTexture.width] = Color.white;
                        } else { 
                            maskImgMatrix[x + y * _scotomaTexture.width] = (px_in[x + y * _scotomaTexture.width].grayscale < threshold) ? Color.black : Color.white;
                        }
                        isMask[x, y] = (maskImgMatrix[x + y * _scotomaTexture.width] == Color.black) ? true : false;
                    }
                }
                inpainterTexture.SetPixels(maskImgMatrix);
                inpainterTexture.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
            }
        }

        private void compute()
        {
            if (useComputeShader && SystemInfo.supportsComputeShaders)
            {

                // = (ComputeShader)Resources.Load("MyFirstComputer");
                int kernelIndex = shader2.FindKernel("CSMain");

                // generate texture
                shader2.SetFloat("Threshold", threshold);
                shader2.SetTexture(kernelIndex, "InputTexture", inpainterTexture);
                shader2.SetTexture(kernelIndex, "Result", _inpainterTexture);
                shader2.Dispatch(kernelIndex, _inpainterTexture.width, _inpainterTexture.height, 1);

                cardinalOffsetsTexture = new Texture2D(inpainterTexture.width, inpainterTexture.height, TextureFormat.ARGB32, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
                RenderTexture.active = _inpainterTexture;
                cardinalOffsetsTexture.wrapMode = TextureWrapMode.Clamp;
                cardinalOffsetsTexture.ReadPixels(new Rect(0, 0, _inpainterTexture.width, _inpainterTexture.height), 0, 0);
                cardinalOffsetsTexture.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
                return;
            }
            cardinalOffsetsImgMatrix = new Color[inpainterTexture.width * inpainterTexture.height];
            for (int x = 0; x < inpainterTexture.width; x++)
            {
                for (int y = 0; y < inpainterTexture.height; y++)
                {
                    if (!isMask[x, y])
                    {
                        cardinalOffsetsImgMatrix[x + y * inpainterTexture.width] = new Color(0f, 0f, 0f, 0f);
                    }
                    else
                    {
                        // count number of steps in each direction until reach a non-masked pixel
                        int x0 = 1;
                        while (isMask[x-x0, y])
                        {
                            x0++;
                        }
                        int x1 = 1;
                        while (isMask[x+x1, y])
                        {
                            x1++;
                        }
                        int y0 = 1;
                        while (isMask[x, y-y0])
                        {
                            y0++;
                        }
                        int y1 = 1;
                        while (isMask[x, y+y1])
                        {
                            y1++;
                        }

                        // convert to 0 - 1
                        float left = (float)x0 / inpainterTexture.width;
                        float right = (float)x1 / inpainterTexture.width;
                        float up = (float)y0 / inpainterTexture.height;
                        float down = (float)y1 / inpainterTexture.height;

                        // store as 32 bit color value
                        cardinalOffsetsImgMatrix[x + y * inpainterTexture.width] = new Color(left, right, up, down);
                    }
                }
            }

            cardinalOffsetsTexture = new Texture2D(inpainterTexture.width, inpainterTexture.height, TextureFormat.ARGB32, false); // Create a new texture ARGB32 and no 
            cardinalOffsetsTexture.wrapMode = TextureWrapMode.Clamp;
            cardinalOffsetsTexture.SetPixels(cardinalOffsetsImgMatrix);
            cardinalOffsetsTexture.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
        }        

        // Update is called once per frame
		protected override void OnUpdate()
        {
            if (useFieldTexture)
            {
                scotomaTexture = fieldLossHandle.overlayTexture;
            }

            if ( (_old_scotomaTexture != scotomaTexture) || (_old_threshold != threshold))
            {
                this.generateFromTexture(scotomaTexture);
            }


            // Gaze-contingent
            Vector2 xy_norm = GazeTracker.GetInstance.xy_norm;
            Material.SetFloat("_MouseX", 1 - xy_norm.x); // Why 1-x???
            Material.SetFloat("_MouseY", xy_norm.y);
            //Debug.Log ("Setting position: " + ((-pos.x+1f)/2f) );
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // null mode
            //Graphics.Blit(source, destination);


            // debug mode
            //Graphics.Blit(_inpainterTexture, destination);

            // simple mode
            Graphics.Blit(source, destination, Material);
        }
		
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myInpainter2_hacked";
		}
    }
}