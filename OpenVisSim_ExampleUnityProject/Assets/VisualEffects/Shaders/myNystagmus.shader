// PJ 13/09/2017

Shader "Hidden/VisSim/myNystagmus"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Displace ("Displace", Vector) = (0.7, 0.0, 0.0, 0.0)
	}

	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half2 _Displace;
		sampler2D _NullingOverlay;
		half _MouseX, _MouseY;

		half4 frag_basic(v2f_img i) : SV_Target
		{
			return tex2D(_MainTex, i.uv + half2(_Displace.x, _Displace.y));
		}

		half4 frag_nulled(v2f_img i) : SV_Target
		{
			// get nulling texture
			float2 degcoords = half2(_MouseX, _MouseY);
			//degcoords = UnityStereoScreenSpaceUVAdjust(degcoords, _Overlay_ST); // is this necessary???
			float4 nulling = tex2D(_NullingOverlay, degcoords);
			float w = 1 - (nulling.x + nulling.y + nulling.z) / 3;

			// square to increase the nulling effect
			w = w * w;

			// return
			return tex2D(_MainTex, i.uv + half2(_Displace.x * w, _Displace.y * w));
		}

	ENDCG


	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		// (0) Basic
		Pass
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag_basic
				#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}

		// (1) With nulling
		Pass
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag_nulled
				#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
	}

	FallBack off
}
