// PJ 13/09/2017
using UnityEngine;

namespace VisSim
{
    public class myBloom : LinkableBaseEffect
        {
            
            public enum Resolution
            {
                Low = 0,
                High = 1,
            }

            public enum BlurType
            {
                Standard = 0,
                Sgx = 1,
            }

			[Linkable, Range(0.0f, 2.5f)]
			public float intensity = 0.75f;

            [Linkable, Range(0.0f, 1.5f)]
            public float threshold = 0.25f;

            [Linkable, Range(0.25f, 5.5f)]
            public float blurSize = 1.0f;

            Resolution resolution = Resolution.Low;
            [Linkable, Range(1, 4)]
            public int blurIterations = 1;

            [Linkable]
            public BlurType blurType = BlurType.Standard;

            protected override void OnUpdate()
            {
            }

            protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                int divider = resolution == Resolution.Low ? 4 : 2;
                float widthMod = resolution == Resolution.Low ? 0.5f : 1.0f;

                Material.SetVector("_Parameter", new Vector4(blurSize * widthMod, 0.0f, threshold, intensity));
                source.filterMode = FilterMode.Bilinear;

                var rtW = source.width / divider;
                var rtH = source.height / divider;

                // downsample
                RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt.filterMode = FilterMode.Bilinear;
                Graphics.Blit(source, rt, Material, 1);

                var passOffs = blurType == BlurType.Standard ? 0 : 2;

                for (int i = 0; i < blurIterations; i++)
                {
                    Material.SetVector("_Parameter", new Vector4(blurSize * widthMod + (i * 1.0f), 0.0f, threshold, intensity));

                    // vertical blur
                    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                    rt2.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(rt, rt2, Material, 2 + passOffs);
                    RenderTexture.ReleaseTemporary(rt);
                    rt = rt2;

                    // horizontal blur
                    rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                    rt2.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(rt, rt2, Material, 3 + passOffs);
                    RenderTexture.ReleaseTemporary(rt);
                    rt = rt2;
                }

                Material.SetTexture("_Bloom", rt);

                Graphics.Blit(source, destination, Material, 0);

                RenderTexture.ReleaseTemporary(rt);
        }

        protected override string GetShaderName()
        {
            return "Hidden/VisSim/myBloom";
        }
    }
    }
