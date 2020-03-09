// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// PJ 13/09/2017

Shader "Hidden/VisSim/myNoise"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NoiseTex("Overlay (RGB)", 2D) = "white" {}
		_Intensity("Magnitude", Float) = 0.0
		_NoiseTex1("Overlay (RGB)", 2D) = "white" {}
		_Tween("Magnitude", Float) = 0.0
		_WarpParams("Frequency (X) Amplitude (Y) Timer (Z)", Vector) = (0, 0, 0, 0)
	}

		CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half4 _MainTex_ST;
		sampler2D _NoiseTex;
		half4 _NoiseTex_ST;
		sampler2D _Overlay;
		half4 _Overlay_ST;

		float _Intensity;

		sampler2D _NoiseTex1;
		float _Tween;
		half3 _WarpParams;


		half4 _MainTex_TexelSize;
		half4 _UV_Transform = half4(1, 0, 0, 1);

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv[2] : TEXCOORD0;
		};

		v2f vert(appdata_img v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);

			o.uv[0] = UnityStereoScreenSpaceUVAdjust(float2(
				dot(v.texcoord.xy, _UV_Transform.xy),
				dot(v.texcoord.xy, _UV_Transform.zw)
				), _Overlay_ST);

				#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0.0)
				o.uv[0].y = 1.0 - o.uv[0].y;
				#endif

			o.uv[1] = UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);
			return o;
		}

		half4 frag_neutral(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);


			half2 t = i.uv;
			t.x += sin(_WarpParams.z + t.x * _WarpParams.x) * _WarpParams.y;
			t.y += cos(_WarpParams.z + t.y * _WarpParams.x) * _WarpParams.y - _WarpParams.y;

			half4 noise = tex2D(_NoiseTex, t);
			const float4 rgb2grey = float4(0.299, 0.587, 0.114, 1.0);
			float n = saturate(noise.x*rgb2grey.x + noise.y*rgb2grey.y + noise.z*rgb2grey.z)*8;

			half4 noise1 = tex2D(_NoiseTex1, t);
			n = (1-_Tween)*n + (_Tween*saturate(noise1.x*rgb2grey.x + noise1.y*rgb2grey.y + noise1.z*rgb2grey.z) * 8);

			return lerp(color, (color + n)/16, _Intensity);
		}

		half4 frag_multiply(v2f_img i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);

			half4 noise = tex2D(_NoiseTex, i.uv);
			const float4 rgb2grey = float4(0.299, 0.587, 0.114, 1.0);
			float n = saturate(noise.x*rgb2grey.x + noise.y*rgb2grey.y + noise.z*rgb2grey.z);

			half4 noise1 = tex2D(_NoiseTex1, i.uv);
			n = (1 - _Tween)*n + (_Tween*saturate(noise1.x*rgb2grey.x + noise1.y*rgb2grey.y + noise1.z*rgb2grey.z));

			return lerp(color, color * n, _Intensity);
		}


		ENDCG

		SubShader
	{
		ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			// (0) Monochrome
			Pass
		{
			CGPROGRAM

#pragma vertex vert_img
#pragma fragment frag_neutral
#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}

			// (1) Colored
			Pass
		{
			CGPROGRAM

#pragma vertex vert
#pragma fragment frag_multiply
#pragma fragmentoption ARB_precision_hint_fastest

			ENDCG
		}


	}

	FallBack off
}
