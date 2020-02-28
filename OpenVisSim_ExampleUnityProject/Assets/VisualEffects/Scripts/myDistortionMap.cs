using System;
using UnityEngine;

namespace VisSim
{
    [ExecuteInEditMode]
    [AddComponentMenu("Hidden/myDistortionMap")]
    public class myDistortionMap : LinkableBaseEffect
    {

        [TweakableMember(1, 3, "dummy", "Warping")]
        private string dummy = "";

        private Texture2D warpTextureX;
		private Texture2D warpTextureY;

        // init filter options
        /*
        private float[] warp_x = { 0.5f, 0.65f, 0f, 0f };
        private float[] warp_y = { 0.5f, 0.65f, 0f, 0f };
        private float[] warp_radius = { 0.15f, 0.15f, 0f, 0f };
        private float[] warp_magn = { 1f, -1f, 0f, 0f };
        */

        // for sim
        public float[] warp_x = { 0f, 0.33f, 0f, 0f };
        public float[] warp_y = { 0f, 0.66f, 0f, 0f };
        public float[] warp_radius = { 0.15f, 0.15f, 0f, 0f };
        public float[] warp_magn = { 1f, -1f, 0f, 0f };

        // Use this for initialization
        public new void OnEnable() {
            base.OnEnable();

            // generate an initial overlay texture based on default parameters
            this.compute();

            // return focus to GameView to ensure mouse keeps reporting coordinates
            #if UNITY_EDITOR
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                UnityEditor.SceneView.FocusWindowIfItsOpen(T);
#endif


            /*
             //FOR SCREENSHOTS FOR IEEE PAPER
            byte[] bytes = warpTextureX.EncodeToPNG();
            string filename = string.Format("{0}/screenshots/deformmapX_{1}.png",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Saved deformmap to: {0}", filename));


            Texture2D overlayTexture_hiRes = Instantiate(warpTextureX);
            TextureScale.Bilinear(overlayTexture_hiRes, 1600, 1600);
            bytes = overlayTexture_hiRes.EncodeToPNG();
            filename = string.Format("{0}/screenshots/deformmapX_hiRes_{1}.png",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Saved deformmap to: {0}", filename));


            bytes = warpTextureY.EncodeToPNG();
            filename = string.Format("{0}/screenshots/deformmapY_{1}.png",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Saved deformmap to: {0}", filename));


            overlayTexture_hiRes = Instantiate(warpTextureY);
            TextureScale.Bilinear(overlayTexture_hiRes, 1600, 1600);
            bytes = overlayTexture_hiRes.EncodeToPNG();
            filename = string.Format("{0}/screenshots/deformmapY_hiRes_{1}.png",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Saved deformmap to: {0}", filename));
            */

        }

        public float[] getWarpX()
        {
            return warp_x;
        }
        public void setWarpX(float[] val)
        {
            warp_x = val;
        }
        public float[] getWarpY()
        {
            return warp_y;
        }
        public void setWarpY(float[] val)
        {
            warp_y = val;
        }
        public float[] getWarpRadius()
        {
            return warp_radius;
        }
        public void setWarpRadius(float[] val)
        {
            warp_radius = val;
        }
        public float[] getWarpMagnitude()
        {
            return warp_magn;
        }
        public void setWarpMagnitude(float[] val)
        {
            warp_magn = val;
        }

        public void compute()
        {
            int width_px = 512;
            int height_px = 512;
            //Ensure size is a power of two: NB this is extremely important, as mipmaps will only be generated for rendertextures that are a power of two!!
            if (width_px != Mathf.ClosestPowerOfTwo(width_px))
            {
                Debug.LogFormat("WARNING, width ({0}) must be a power of two. Will round upwards to {1}", width_px, Mathf.NextPowerOfTwo(width_px));
                width_px = Mathf.ClosestPowerOfTwo(width_px);
            }
            if (height_px != Mathf.ClosestPowerOfTwo(height_px))
            {
                Debug.LogFormat("WARNING, height ({0}) must be a power of two. Will round upwards to {1}", height_px, Mathf.NextPowerOfTwo(height_px));
                height_px = Mathf.ClosestPowerOfTwo(height_px);
            }

            // compute
            float[,] vx = new float[width_px, height_px];
            float[,] vy = new float[width_px, height_px];
            float SQUARE_ROOT_2 = Mathf.Sqrt(2);
            for (int x = 0; x < width_px; x++)
            {
                for (int y = 0; y < height_px; y++)
                {
                    for (int i = 0; i < warp_x.Length; i++)
                    {
                        if (warp_magn[i] != 0)
                        { 
                            float offset_x = (float)x/(float)width_px - warp_x[i];
                            float offset_y = (float)y/(float)width_px - warp_y[i];

                            float percent = 1.0f - ((warp_radius[i] - Mathf.Sqrt(Mathf.Pow(offset_x, 2) + Mathf.Pow(offset_y, 2))) / warp_radius[i]) * warp_magn[i] / SQUARE_ROOT_2;
                    
                            if (warp_magn[i] < 0)
                            { 
                                percent = Mathf.Max(1f, percent);
                            } else { 
                                percent = Mathf.Min(1f, percent);
                            }

                            vx[x, y] += offset_x - offset_x * percent;
                            vy[x, y] += offset_y - offset_y * percent;
                        }
                    }
                }
            }
            
            // Round and make texture
            Color[] imgMatrix_x = new Color[width_px * height_px];
            Color[] imgMatrix_y = new Color[width_px * height_px];
            for (int x = 0; x < width_px; x++)
            {
                for (int y = 0; y < height_px; y++)
                {
                    // 1 bound to range -.1 to 0.1
                    vx[x, y] = Mathf.Max(-0.1f, Mathf.Min(0.1f, vx[x,y]));
                    vy[x, y] = Mathf.Max(-0.1f, Mathf.Min(0.1f, vy[x, y]));

                    // 2 - convert to texture
                    imgMatrix_x[x + y * height_px] = Color.white * (vx[x, y] * 5f + 0.5f); // convert to range 0 to 1
                    imgMatrix_y[x + y * height_px] = Color.white * (vy[x, height_px-y-1] * 5f + 0.5f);
                }
            }
            // make textures
            //Texture2D warpTextureX = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            warpTextureX = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            warpTextureX.SetPixels(imgMatrix_x);
            warpTextureX.Apply(false); // actually apply all SetPixels, don't recalculate mip levels
            //Texture2D warpTextureY = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            warpTextureY = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            warpTextureY.SetPixels(imgMatrix_y);
            warpTextureY.Apply(false); // actually apply all SetPixels, don't recalculate mip levels

            // for debugging: draw texture to screen!
            //GameObject currentLoc = GameObject.Find("Cube (Street Scene)"); //("Cube (Texture2D)");
            //currentLoc.GetComponent<Renderer>().material.mainTexture = warpTextureX;

            // set texture as overlays in shader
            Material.SetTexture("_WarpTextureX", warpTextureX);
            Material.SetTexture("_WarpTextureY", warpTextureY);


        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, Material);
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
			/*

			// Get convergence data
			Fove.Managed.SFVR_GazeConvergenceData convergence = FoveInterface2.GetFVRHeadset ().GetGazeConvergence ();

			// use Ray to get world space coordinate: 
			Vector3 o = new Vector3 (convergence.ray.origin.x, convergence.ray.origin.y, convergence.ray.origin.z);
			Vector3 d = new Vector3 (convergence.ray.direction.x, convergence.ray.direction.y, convergence.ray.direction.z);
			Vector3 pos = o + d * 1.0f;
			//material.SetFloat("_MouseX", 1f-(pos.x+0.5f)); 
			//material.SetFloat("_MouseY", 1f-(pos.y+0.5f));
			Material.SetFloat("_MouseX", (-pos.x+1f)/2f);
            Material.SetFloat("_MouseY", (-pos.y+1f)/2f);
            */

            // Gaze-contingent
            Vector2 xy_norm = GazeTracker.GetInstance.xy_norm;
            Material.SetFloat("_MouseX", 1 - xy_norm.x); // Why 1-x???
            Material.SetFloat("_MouseY", xy_norm.y);
        }
		
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myDistortionMap";
		}
    }
}
