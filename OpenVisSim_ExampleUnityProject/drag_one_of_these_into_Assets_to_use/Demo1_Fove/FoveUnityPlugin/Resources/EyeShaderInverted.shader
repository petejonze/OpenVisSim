Shader "Fove/EyeShaderInverted"
{
	Properties {
		_Tex1 ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Tex2 ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader {
		Pass {  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				sampler2D _Tex1;
				float4 _Tex1_ST;
				sampler2D _Tex2;
				float4 _Tex2_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _Tex1);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					float mask = 0;
					mask = i.texcoord.x;
					mask = saturate(saturate(max(mask, 0.5) - 0.5) * 10000);

					i.texcoord.x *= 2;
					i.texcoord.y = 1 - i.texcoord.y;

					fixed4 col = (1 - mask) * tex2D(_Tex2, i.texcoord);
					i.texcoord.x -= 1;
					col += (mask) * tex2D(_Tex1, i.texcoord);
					return col;
				}
			ENDCG
			Cull Off
		}
	}
}
