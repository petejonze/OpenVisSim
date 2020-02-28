// PJ 13/09/2017

Shader "Hidden/VisSim/myInpainter2"
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Overlay("Overlay Texture", 2D) = "white" {}
		_MouseX("Mouse X Position (Normalized 0 to 1)", Float) = 0.5
		_MouseY("Mouse Y Position (Normalized 0 to 1)", Float) = 0.5
		_ViewDist_m("Viewing distance in meters", Float) = 2.4815
	}
		
	SubShader
	{
		AlphaTest Off
		Cull Back
		Lighting Off
		ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert // Tells the cg to use a vertex-shader called vert	
			#pragma fragment frag // Tells the cg to use a fragment-shader called frab
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _Overlay;
			half4 _Overlay_ST;
			half _MouseX, _MouseY;
			float _ViewDist_m;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv[2] : TEXCOORD0;
				float4 vertexPos : SV_POSITION;
			};


			v2f vert(appdata_t v)
			{
				v2f o;

				// basic info
				o.vertexPos = UnityObjectToClipPos(v.vertex);
				o.uv[0] = v.texcoord.xy; // ALT: UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _Overlay_ST);
				o.uv[1] = v.texcoord.xy; // ALT: UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);

				return o;
			};

			fixed4 frag(v2f i) : SV_Target
			{
				// correct mouse y-coordinates if on a platform where '0' is the top (e.g., Direct3D-like)
				# if UNITY_UV_STARTS_AT_TOP
					_MouseY = 1.0 - _MouseY;
				# endif
			
				// fetch sample from degradation map
				float2 offset = float2(unity_CameraProjection[0][2] / _ViewDist_m, unity_CameraProjection[1][2] / _ViewDist_m); // offset due to left/right eye projection [NB: pretty sure this is a really stupid hack, but it is better than nothing...]
				float2 mouseOffset = (half2(0.5, 0.5) - half2(_MouseX, _MouseY)); // map 0-1 (min/max of screen) to -.5 to +.5
				float4 nearestVals = tex2D(_Overlay, (i.uv[0].xy - mouseOffset) + offset); //0.13
				//float w = 1.0 - saturate(degradation.x*rgb2grey.x + degradation.y*rgb2grey.y + degradation.z*rgb2grey.z);

				// debugging
				//if ((nearestVals.x + nearestVals.y + nearestVals.z + nearestVals.w) < 0.01)
				if (nearestVals.x< 0.0001 || nearestVals.y < 0.0001 || nearestVals.z < 0.0001 || nearestVals.w < 0.0001)
				{
					return tex2D(_MainTex, i.uv[0].xy); // return original, unmolested
				} else {
					# if UNITY_UV_STARTS_AT_TOP
					//nearestVals.z = 1.0 - nearestVals.z;
					//nearestVals.w = 1.0 - nearestVals.w;
					# endif

					//return nearestVals;
					float4 a = tex2D(_MainTex, float2(i.uv[0].x - nearestVals.x, i.uv[0].y));
					float4 b = tex2D(_MainTex, float2(i.uv[0].x + nearestVals.y, i.uv[0].y));
					float4 c = tex2D(_MainTex, float2(i.uv[0].x, i.uv[0].y - nearestVals.z));
					float4 d = tex2D(_MainTex, float2(i.uv[0].x, i.uv[0].y + nearestVals.w));


					// could be done 'offline' once, and loaded in
					float a_d = pow(1.0 / nearestVals.x, 2);
					float b_d = pow(1.0 / nearestVals.y, 2);
					float c_d = pow(1.0 / nearestVals.z, 2);
					float d_d = pow(1.0 / nearestVals.w, 2);
					float sumval = (a_d + b_d + c_d + d_d);
					float a_w = a_d / sumval;
					float b_w = b_d / sumval;
					float c_w = c_d / sumval;
					float d_w = d_d / sumval;

					return float4( a.x*a_w + b.x*b_w + c.x*c_w + d.x*d_w,   a.y*a_w + b.y*b_w + c.y*c_w + d.y*d_w,     a.z*a_w + b.z*b_w + c.z*c_w + d.z*d_w,     a.w*a_w + b.w*b_w + c.w*c_w + d.w*d_w);
				}
			}

			ENDCG
		}
	}
}
