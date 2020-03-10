// PJ 13/09/2017

Shader "Hidden/VisSim/myLed"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Params("Scale (X) Ratio (Y) Brightness (Z) Shape (W)", Vector) = (80, 1, 1, 1.5)
		_Margin("Blank Margin", Float) = 0.0
	}

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 
				#include "UnityCG.cginc"
				#include "./OVShelperFuncs.cginc"

				sampler2D _MainTex;
				half4 _Params;
				float _Margin;

				half4 frag(v2f_img i) : SV_Target
				{
					if (i.uv.x < _Margin || i.uv.y < _Margin || (1.0 - i.uv.x) < _Margin || (1.0 - i.uv.y) < _Margin)
					{
						return float4(0.0, 0.0, 0.0, 1.0);
					}
					half4 color = pixelate(_MainTex, i.uv, _Params.x, _Params.y) * _Params.z;
					half2 coord = i.uv * half2(_Params.x, _Params.x / _Params.y);
					half2 mv = abs(sin(coord * PI)) * _Params.w;
					half s = mv.x * mv.y;
					half c = step(s, 1.0);
					color = ((1 - c) * color) + ((color * s) * c);
					return color;
				}

			ENDCG
		}
	}

	FallBack off
}
