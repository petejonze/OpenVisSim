// PJ 13/09/2017

Shader "Hidden/VisSim/myScintillate"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Params ("Seed (X) Strength (Y) Lum Contribution (Z)", Vector) = (0, 0, 0, 0)
		_Boost("Saturation Boost (Float)", Float) = 1.0
	}

	CGINCLUDE

		#include "UnityCG.cginc"
		#include "./OVShelperFuncs.cginc"

		sampler2D _MainTex;
		half3 _Params;
		sampler2D _MaskOverlay;
		half _MouseX, _MouseY;
		half _Boost;


		half4 frag_mono(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			float n = simpleNoise(i.uv + _Params.x) * 2.0;
			return lerp(color, color * n, _Params.y);
		}

		half4 frag_colored(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			float n = simpleNoise_fracLess(i.uv + _Params.x);
			float nr = frac(n) * 2.0;
			float ng = frac(n * 1.2154) * 2.0;
			float nb = frac(n * 1.3453) * 2.0;
			float na = frac(n * 1.3647) * 2.0;
			return lerp(color, color * half4(nr, ng, nb, na), _Params.y);
		}

		half4 frag_mono_lum(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			float n = simpleNoise(i.uv + _Params.x) * 2.0;
			half lum = luminance(color.rgb);
			return lerp(color, color * n, _Params.y * (1.0 - lerp(0.0, lum, _Params.z)));
		}

		half4 frag_colored_lum(v2f_img i) : SV_Target
		{
			// get this fragment
			half4 color = tex2D(_MainTex, i.uv);

			// make texture coordinate mouse-contingent
			// correct mouse y-coordinates if on a platform where '0' is the top (e.g., Direct3D-like)
			# if UNITY_UV_STARTS_AT_TOP
				_MouseY = 1.0 - _MouseY;
			# endif
			float2 mouseOffset = (half2(0.5, 0.5) - half2(_MouseX, _MouseY)); // map 0-1 (min/max of screen) to -.5 to +.5
			float2 degcoords = i.uv - mouseOffset;
			degcoords = degcoords*.5 + .25; // map 0-1 to 0.25-0.75 (i.e., since deg tex is twice as big) [this has to be a hack! must be a better way of doing this...)

			// is this necessary???
			//degcoords = UnityStereoScreenSpaceUVAdjust(degcoords, _Overlay_ST);

			// get mask
			float4 mask = tex2D(_MaskOverlay, degcoords);

			// if mask is not black then apply noise effect, otherwise simply return fragment unmolested
			if (mask.x > 0.5)
			{
				// increase saturation too
				half3 hsv = RGBtoHSV(color.rgb);
				half s = 2.0 * hsv.y; // PJ 1.0 => 2.0
				color.rgb = HSVtoRGB(half3(hsv.x, s * _Boost, hsv.z));

				// generate and apply noise
				float n = simpleNoise_fracLess(i.uv + _Params.x);
				float nr = frac(n) * 2.0;
				float ng = frac(n * 1.2154) * 2.0;
				float nb = frac(n * 1.3453) * 2.0;
				float na = frac(n * 1.3647) * 2.0;
				half lum = luminance(color.rgb);
				return lerp(color, color * half4(nr, ng, nb, na), _Params.y * (1.0 - lerp(0.0, lum, _Params.z)));
			}
			else
			{
				return color;
			}
		}

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		// (0) Monochrome
		Pass
		{			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag_mono
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		// (1) Colored
		Pass
		{			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag_colored
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		// (2) Monochrome - Lum Contrib
		Pass
		{			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag_mono_lum
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

		// (3) Colored - Lum Contrib
		Pass
		{			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag_colored_lum
				#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}
	}

	FallBack off
}
