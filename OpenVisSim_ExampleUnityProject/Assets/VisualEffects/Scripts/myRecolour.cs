using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace VisSim
{
	[ExecuteInEditMode]
	[AddComponentMenu("Hidden/myRecolour")]
	public class myRecolour : LinkableBaseEffect
	{
        public enum AnomolyType
        {
            Protanomaly = 0,
            Deuteranomaly = 1,
            Tritanomaly = 2,
            Monochrome = 3
        }
        [Linkable, TweakableMember(0,3,"Type", "Color Deficiency")]
        public AnomolyType anomType = AnomolyType.Protanomaly;

        [TweakableMember(0.0f, 1.0f, "Intensity", "Color Deficiency")]
        [Linkable, Range(0.0f, 1.0f)]
		public float severityIndex = 0.0f;

        //#if UNITY_EDITOR
            private float _old_severityIndex = 0.0f;
            private AnomolyType _old_anomType = AnomolyType.Protanomaly;
        //#endif

        // Init transformation matrix
        private Matrix4x4 T = new Matrix4x4();

        private readonly float[,,] T_Protanomaly = new float[11, 3, 3]
        {
            {
                {1f,           0f,           0f},
                {0f,           1f,           0f},
                {0f,           0f,           1f},
            },
            {
                {0.856167f,    0.182038f,    -0.038205f},
                {0.029342f,    0.955115f,    0.015544f},
                {-0.00288f,    -0.001563f,   1.004443f},
            },
            {
                {0.734766f,    0.334872f,    -0.069637f},
                {0.05184f,     0.919198f,    0.028963f},
                {-0.004928f,   -0.004209f,   1.009137f},
            },
            {
                {0.630323f,    0.465641f,    -0.095964f},
                {0.069181f,    0.890046f,    0.040773f},
                {-0.006308f,   -0.007724f,   1.014032f},
            },
            {
                {0.539009f,    0.579343f,    -0.118352f},
                {0.082546f,    0.866121f,    0.051332f},
                {-0.007136f,   -0.011959f,   1.019095f},
            },
            {
                {0.458064f,    0.679578f,    -0.137642f},
                {0.092785f,    0.846313f,    0.060902f},
                {-0.007494f,   -0.016807f,   1.024301f},
            },
            {
                {0.38545f,     0.769005f,    -0.154455f},
                {0.100526f,    0.829802f,    0.069673f},
                {-0.007442f,   -0.02219f,    1.029632f},
            },
            {
                {0.319627f,    0.849633f,    -0.169261f},
                {0.106241f,    0.815969f,    0.07779f},
                {-0.007025f,   -0.028051f,   1.035076f},
            },
            {
                {0.259411f,    0.923008f,    -0.18242f},
                {0.110296f,    0.80434f,    0.085364f},
                {-0.006276f,   -0.034346f,   1.040622f},
            },
            {
                {0.203876f,    0.990338f,    -0.194214f},
                {0.112975f,    0.794542f,    0.092483f},
                {-0.005222f,   -0.041043f,   1.046265f},
            },
            {
                {0.152286f,    1.052583f,    -0.204868f},
                {0.114503f,    0.786281f,    0.099216f},
                {-0.003882f,   -0.048116f,   1.051998f},
            }
        };
        private readonly float[,,] T_Deuteranomaly = new float[11, 3, 3]
        {
            {
                {1f,           0f,           0f},
                {0f,           1f,           0f},
                {0f,           0f,           1f},
            },
            {
                {0.866435f,    0.177704f,    -0.044139f},
                {0.049567f,    0.939063f,    0.01137f},
                {-0.003453f,   0.007233f,    0.99622f},
            },
            {
                {0.760729f,    0.319078f,    -0.079807f},
                {0.090568f,    0.889315f,    0.020117f},
                {-0.006027f,   0.013325f,    0.992702f},
            },
            {
                {0.675425f,    0.43385f,     -0.109275f},
                {0.125303f,    0.847755f,    0.026942f},
                {-0.00795f,    0.018572f,    0.989378f},
            },
            {
                {0.605511f,    0.52856f,     -0.134071f},
                {0.155318f,    0.812366f,    0.032316f},
                {-0.009376f,   0.023176f,    0.9862f},
            },
            {
                {0.547494f,    0.607765f,    -0.155259f},
                {0.181692f,    0.781742f,    0.036566f},
                {-0.01041f,    0.027275f,    0.983136f},
            },
            {
                {0.498864f,    0.674741f,    -0.173604f},
                {0.205199f,    0.754872f,    0.039929f},
                {-0.011131f,   0.030969f,    0.980162f},
            },
            {
                {0.457771f,    0.731899f,    -0.18967f},
                {0.226409f,    0.731012f,    0.042579f},
                {-0.011595f,   0.034333f,    0.977261f},
            },
            {
                {0.422823f,    0.781057f,    -0.203881f},
                {0.245752f,    0.709602f,    0.044646f},
                {-0.011843f,   0.037423f,    0.974421f},
            },
            {
                {0.392952f,    0.82361f,     -0.216562f},
                {0.263559f,    0.69021f,     0.046232f},
                {-0.01191f,    0.040281f,    0.97163f},
            },
            {
                {0.367322f,    0.860646f,    -0.227968f},
                {0.280085f,    0.672501f,    0.047413f},
                {-0.01182f,    0.04294f,     0.968881f},
            }
        };
        private readonly float[,,] T_Tritanomaly = new float[11, 3, 3]
        {
            {
                {1f,           0f,           0f},
                {0f,           1f,           0f},
                {0f,           0f,           1f},
            },
            {
                {0.92667f,     0.092514f,    -0.019184f},
                {0.021191f,    0.964503f,    0.014306f},
                {0.008437f,    0.054813f,    0.93675f},
            },
            {
                {0.89572f,     0.13333f,    -0.02905f},
                {0.029997f,    0.9454f,     0.024603f},
                {0.013027f,    0.104707f,   0.882266f},
            },
            {
                {0.905871f,    0.127791f,    -0.033662f},
                {0.026856f,    0.941251f,    0.031893f},
                {0.01341f,     0.148296f,    0.838294f},
            },
            {
                {0.948035f,    0.08949f,    -0.037526f},
                {0.014364f,    0.946792f,    0.038844f},
                {0.010853f,    0.193991f,    0.795156f},
            },
            {
                {1.017277f,    0.027029f,    -0.044306f},
                {-0.006113f,   0.958479f,    0.047634f},
                {0.006379f,    0.248708f,    0.744913f},
            },
            {
                {1.104996f,    -0.046633f,   -0.058363f},
                {-0.032137f,   0.971635f,    0.060503f},
                {0.001336f,    0.317922f,    0.680742f},
            },
            {
                {1.193214f,    -0.109812f,   -0.083402f},
                {-0.058496f,   0.97941f,    0.079086f},
                {-0.002346f,   0.403492f,    0.598854f},
            },
            {
                {1.257728f,    -0.139648f,   -0.118081f},
                {-0.078003f,   0.975409f,    0.102594f},
                {-0.003316f,   0.501214f,    0.502102f},
            },
            {
                {1.278864f,    -0.125333f,   -0.153531f},
                {-0.084748f,   0.957674f,    0.127074f},
                {-0.000989f,   0.601151f,    0.399838f},
            },
            {
                {1.255528f,    -0.076749f,   -0.178779f},
                {-0.078411f,   0.930809f,    0.147602f},
                {0.004733f,    0.691367f,    0.3039f},
            }
        };

        // Use this for initialization
        new void OnEnable ()
        {
            base.OnEnable();

            // Set transformation matrix
            computeTransformationMatrix();
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //#if UNITY_EDITOR
                if (severityIndex != _old_severityIndex || anomType != _old_anomType) {
                    computeTransformationMatrix();
                }
            //#endif

            // Set transformation matrix
            Material.SetMatrix("_ColourTransform", T);

            Graphics.Blit (source, destination, Material);
		}



        // -----------------------------------------------------------------------------
        // ESTABLISHING TRANSFORMATION
        // -----------------------------------------------------------------------------

        private void computeTransformationMatrix()
        {
            // If severity is 0 just cut to the chase and set identity matrix
            if (severityIndex == 0f) {
				T.SetRow (0, new Vector4(1, 0, 0, 0));
				T.SetRow (1, new Vector4(0, 1, 0, 0));
				T.SetRow (2, new Vector4(0, 0, 1, 0));
				T.SetRow (3, new Vector4(0, 0, 0, 1));
            } else {
                // get appropriate matrices
				float[,,] T0 = T_Deuteranomaly;
				switch (anomType)
				{
                case AnomolyType.Protanomaly:
                    T0 = T_Protanomaly;
					break;
				case AnomolyType.Deuteranomaly:
                    T0 = T_Deuteranomaly;
					break;
				case AnomolyType.Tritanomaly:
					T0 = T_Tritanomaly;
					break;
               case AnomolyType.Monochrome:
                    T.SetRow(0, (new Vector4(0.299f, 0.587f, 0.114f, 0))*severityIndex + (new Vector4(1, 0, 0, 0))*(1f-severityIndex));
                    T.SetRow(1, (new Vector4(0.299f, 0.587f, 0.114f, 0))*severityIndex + (new Vector4(0, 1, 0, 0))*(1f-severityIndex));
                    T.SetRow(2, (new Vector4(0.299f, 0.587f, 0.114f, 0))*severityIndex + (new Vector4(0, 0, 1, 0))*(1f-severityIndex));
                    T.SetRow(3, new Vector4(0, 0, 0, 1));
                    break;
				default:
					Console.WriteLine("??????");
					break;
				};

                if (anomType != AnomolyType.Monochrome)
                {
                    // determine interpolation weight
                    int nElements = 11;
                    int i1 = (int)Mathf.Ceil(severityIndex * (nElements - 1));
                    int i0 = i1 - 1;
                    float w = (severityIndex * (nElements - 1)) - i0;

                    // interpolate to find appropriate matrix multiplication coefficients
                    T.SetRow (0, new Vector4(Mathf.Lerp(T0[i0, 0, 0], T0[i1, 0, 0], w), Mathf.Lerp(T0[i0, 0, 1], T0[i1, 0, 1], w), Mathf.Lerp(T0[i0, 0, 2], T0[i1, 0, 2], w), 0.0f));
				                    T.SetRow (1, new Vector4(Mathf.Lerp(T0[i0, 1, 0], T0[i1, 1, 0], w), Mathf.Lerp(T0[i0, 1, 1], T0[i1, 1, 1], w), Mathf.Lerp(T0[i0, 1, 2], T0[i1, 1, 2], w), 0.0f));
				                    T.SetRow (2, new Vector4(Mathf.Lerp(T0[i0, 2, 0], T0[i1, 2, 0], w), Mathf.Lerp(T0[i0, 2, 1], T0[i1, 2, 1], w), Mathf.Lerp(T0[i0, 2, 2], T0[i1, 2, 2], w), 0.0f));
				                    T.SetRow (3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                }
            }


            // if in editor store values so know whether effect has been updated
            //#if UNITY_EDITOR
                _old_severityIndex = severityIndex;
                _old_anomType = anomType;
            //#endif
        }
		
				
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myRecolour";
		}
    }
}