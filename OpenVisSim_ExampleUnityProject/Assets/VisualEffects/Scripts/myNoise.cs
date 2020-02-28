using System;
using UnityEngine;

namespace VisSim
{
    
    public class myNoise : LinkableBaseEffect
    {

        [Linkable, Range(0.0f, 1.0f)]
        public float intensity = 1.0f;

        // FastNoise params
        private const String tweaklabel = "z Advanced: Complex Noise";
        [TweakableMember(0.0f, 1.0f, "frequency", tweaklabel)]
        public float frequency = 0.01f;
        [TweakableMember(0, 1, "interp", tweaklabel)]
        public FastNoise.Interp interp = FastNoise.Interp.Quintic;
        [TweakableMember(0, 1, "type", tweaklabel)]
        public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Simplex;
        [TweakableMember(0, 20, "octaves", tweaklabel)]
        public int octaves = 3;
        [TweakableMember(0.0f, 10.0f, "lacunarity", tweaklabel)]
        public float lacunarity = 2.0f;
        [TweakableMember(0.0f, 2.0f, "gain", tweaklabel)]
        public float gain = 0.5f;
        public FastNoise.FractalType fractalType = FastNoise.FractalType.FBM;

        // internal
        private Texture2D[] tex;
        private int counter = 0;

        // Use this for initialization
        public new void OnEnable() {
            base.OnEnable();

            //
            int width_px =Screen.width;
            int height_px = Screen.height;
            int N = 10;

            //
            FastNoise fNoise = new FastNoise(); // Create a FastNoise object
            fNoise.SetFrequency(frequency);
            fNoise.SetInterp(interp);
            fNoise.SetNoiseType(noiseType);
            fNoise.SetFractalOctaves(octaves);
            fNoise.SetFractalLacunarity(lacunarity);
            fNoise.SetFractalGain(gain);
            fNoise.SetFractalType(fractalType);

            //
            Color32[] pixels = new Color32[width_px * height_px];
            tex = new Texture2D[N];
            for (int i = 0; i < N; i++)
            {
                fNoise.SetSeed((int)UnityEngine.Random.Range(0.0f, 1000.0f));
                int index = 0;
                for (int y = 0; y < height_px; y++)
                {
                    for (int x = 0; x < width_px; x++)
                    {
                        byte noise = (byte)Mathf.Clamp(fNoise.GetNoise(x * 2f, y * 2f) * 127.5f + 127.5f, 0f, 255f);
                        pixels[index++] = new Color32(noise, noise, noise, 255);
                    }
                }
                tex[i] = new Texture2D(width_px, height_px);
                tex[i].SetPixels32(pixels);
                tex[i].Apply(false);
            }

            // for debugging: draw texture to screen!
            //GameObject currentLoc = GameObject.Find("Cube (Street Scene)"); //("Cube (Texture2D)");
            //currentLoc.GetComponent<Renderer>().material.mainTexture = tex[0];
        }

        // Called by camera to apply image effect
        //Vector4 UV_Transform = new Vector4(1, 0, 0, 1);
        float tween = 0f;
        int counter1 = 1;
        [Range(0.0f, 1.0f)]
        public float speed = 1f;

        private float wTimer = 0f;
        public float wSpeed = 1f;
        public float wFrequency = 12f;
        public float wAmplitude = 0.01f;


        // Update is called once per frame
		protected override void OnUpdate()
        {
            // Reset the timer after a while, some GPUs don't like big numbers
            if (wTimer > 1000f)
                wTimer -= 1000f;

            // Increment timer
            wTimer += wSpeed * Time.deltaTime;
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            tween += speed;

            //
            if (tween >= 1f)
            { 
                counter++;
                if (counter > 9) { counter = 0; }
                counter1++;
                if (counter1 > 9) { counter1 = 0; }
                tween = 0f;
            }

            // set params
            //Material.SetVector("_UV_Transform", UV_Transform);
            //Material.SetFloat("_Intensity", intensity);
            Material.SetTexture("_NoiseTex", tex[counter]);
            //Material.SetTexture("_MainTex", source);

            Material.SetFloat("_Intensity", intensity);

            
            Material.SetTexture("_NoiseTex1", tex[counter1]);
            Material.SetFloat("_Tween", tween);

            Material.SetVector("_WarpParams", new Vector3(wFrequency, wAmplitude, wTimer));

            // Blit
            Graphics.Blit(source, destination, Material, 0);
        }

				
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myNoise";
		}
    }
}
