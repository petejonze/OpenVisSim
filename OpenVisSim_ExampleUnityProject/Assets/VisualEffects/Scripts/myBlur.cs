// PJ
// TODO - get FOV and screenwidth info from the hardware directly? Or at least from the Fove Interface object?

using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace VisSim
{
    
    public class myBlur : LinkableBaseEffect
    {

        
        // amount of blurring (in acuity/cycles-per-degree units)
        [Linkable, Range(0.01f, 10.0f), TweakableMember(0.01f, 10.0f, "Max CPD", "Acuity Loss (Blur)")]
        public float maxCPD = 30.0f; // reminder that 1 logMar [top row of chart] is equal to about 3 cpd, 0==30, -0.2==47.5 [30/exp10(1.0)]

        // amount of blurring (in computer/kernel-size units)
        [Linkable, Range(0.001f, 10.0f)]
        public float kernalSigma = 3.0f;
        public float getkernalSigma() { return kernalSigma; } // make kernalSigma readonly

        // geometry info
        [Linkable, Range(100, 10000)]
        public int screenWidth_px = 2560;
        /*[Linkable, Range(0.001f, 60.0f)]
        public float screenWidth_cm = 3.0f;
        [Linkable, Range(0.001f, 60.0f)]
        public float viewingDist_cm = 3.0f;*/
        [Linkable, Range(1f, 180.0f)]
        public float viewingAngle_deg = 80.0f;

        // internal
        [Range(0, 2)]
        private int downsample = 0;
        [Range(0.0f, 10.0f)]
        private float blurSize = 1.0f;
        private const float MAX_KERNAL_SIGMA = 20f;
        private int kernelSize;
        private float widthMod = 1.0f;
        private float[] floatArray;

        //#if UNITY_EDITOR
        private float _old_kernalSigma = 3.0f;
        private float _old_maxCPD = 30.0f;
        //#endif

        // Use this for initialization
        public new void OnEnable()
        {
            // init blur kernel
            computeBlurKernel();

            // call base method to enable effect
            base.OnEnable();
        }

        private void computeBlurKernel()
        {
            // init
            widthMod = 1.0f / (1.0f * (1 << downsample));
            
            // compute kernalSigma
            float pixel_per_dg = screenWidth_px / viewingAngle_deg; // screenWidth_dg
            //Debug.Log("pixel_per_dg: " + pixel_per_dg);

            double sigma = 1 / (2 * Math.PI * maxCPD);
            sigma = Math.Sqrt(2 * Math.Log(2)) * sigma; // scale so that cutoff specifies 50 % decrement(otherwise will be targetting 60.7 %)
            sigma = sigma * pixel_per_dg; // convert to pixels
            kernalSigma = (float)sigma;
            //Debug.Log("kernalSigma: " + kernalSigma);
            
            //
            if (kernalSigma > MAX_KERNAL_SIGMA)
            {
                kernalSigma = MAX_KERNAL_SIGMA;
                maxCPD = (float)((Math.Sqrt(2) * pixel_per_dg * Math.Sqrt(Math.Log(2))) / (2 * kernalSigma * Math.PI));
            }

            // set Gaussian kernel
            kernelSize = (int)Mathf.Round(4.0f * kernalSigma + 1.0f);
            float[] gaussianKernel = GaussianKernel(kernalSigma, kernelSize, true);
            floatArray = new float[100];
            for (int i = 0; i < kernelSize; i++)
            {
                floatArray[i] = gaussianKernel[i];
            }

            // if in editor store values so know whether effect has been updated
            //#if UNITY_EDITOR
                _old_kernalSigma = kernalSigma;
                _old_maxCPD = maxCPD;
            //#endif
        }

        /// <summary>
        /// Procedural gaussian kernel. More accurate than the const ones below, but it takes time to create it.
        /// </summary>
        /// <param name="sigma">Variance, not the square</param>
        /// <param name="size">(HALF) Width or length of the kernel</param>
        /// <param name="normalize">Weither the kernel must be normalized</param>
        /// <returns>Gaussian kernel</returns>
        private static float[] GaussianKernel(float sigma, int size, bool normalize = true)
        {
            float[] kernel = new float[size];

            float sigmaSqr2 = sigma * sigma * 2f;
            float sigmaSqr2piInv = 1f / Mathf.Sqrt((float)Math.PI * sigmaSqr2);

            int k = size / 2;

            for (int i = 0; i < size; i++)
            {
                int ki = i - k;
                float exp = Mathf.Exp(-((ki*ki) / sigmaSqr2));
                kernel[i] = sigmaSqr2piInv * exp;
            }

            if (normalize)
            {
                float sum = 0f;
                foreach (float f in kernel)
                    sum += f;

                for (int i = 0; i < kernel.Length; i++)
                    kernel[i] /= sum;
            }

            return kernel;
        }

        // Update is called once per frame
		protected override void OnUpdate()
        {
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // update blur kernel if necessary
            //#if UNITY_EDITOR
            if (kernalSigma != _old_kernalSigma || maxCPD != _old_maxCPD)
            {
                computeBlurKernel();
            }
            //#endif

            // set shader params
            Material.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
            Material.SetFloatArray("_myCurve", floatArray);
            Material.SetInt("_kernelSize", kernelSize);


            // downsample
            source.filterMode = FilterMode.Bilinear;
            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;
            RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, rt, Material, 0);
            Material.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, Material, 0);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, Material, 1);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // Blit
            Graphics.Blit(rt, destination);

            // Free memory
            RenderTexture.ReleaseTemporary(rt);
        }

		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myBlur";
		}
    }
}
