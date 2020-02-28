// PJ 13/09/2017
// A combination of:
//	(1) the 'screenoverlay' mode from BlendModesOverlay.shader
//	(2) the 'simple' mode from ColorFX/Wiggle.shader
// PJ<petejonze@gmail.com> 20/04/2017

Shader "Hidden/VisSim/myFieldLoss"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Overlay("Overlay Texture", 2D) = "white" {}
		_LODlevel("LOD level", Float) = 0.0
		_MaxLODlevel("Max LOD level", Float) = 0.0
		_MouseX("Mouse X Position (Normalized 0 to 1)", Float) = 0.0
		_MouseY("Mouse Y Position (Normalized 0 to 1)", Float) = 0.0
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
		float _LODlevel;
		float _MaxLODlevel;
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

			/*
			float4 screenPos : TEXCOORD2;
			float4 pos : TEXCOORD6;
			float3 viewpos : TEXCOORD7;
			float4 clippos : TEXCOORD8;
			float3 worldpos : TEXCOORD9;
			*/
		};


		v2f vert(appdata_t v)
		{
			v2f o;

			// basic info
			o.vertexPos = UnityObjectToClipPos(v.vertex);
			o.uv[0] = v.texcoord.xy; // ALT: UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _Overlay_ST);
			o.uv[1] = v.texcoord.xy; // ALT: UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);

			// not required
			/*
			o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex.xyz));
			float4 wPos = mul(unity_ObjectToWorld, v.vertex);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.viewpos = wPos.xyz - _WorldSpaceCameraPos;
			o.clippos = o.pos;
			o.worldpos = wPos.xyz;
			*/

			return o;
		};

		fixed4 frag(v2f i) : SV_Target
		{
			// correct mouse y-coordinates if on a platform where '0' is the top (e.g., Direct3D-like)
			# if UNITY_UV_STARTS_AT_TOP
				_MouseY = 1.0 - _MouseY;
			# endif

			// values to convert rgb to grayscale from:
			// Foley et al, "Computer Graphics: Principles and Practice"
			const float4 rgb2grey = float4(0.299, 0.587, 0.114, 1.0);
			//const float4 rgb2grey = float4(0.3333333, 0.3333333, 0.3333333, 0.3333333);

			// fetch sample from degradation map
			float2 offset = float2(unity_CameraProjection[0][2] / _ViewDist_m, unity_CameraProjection[1][2] / _ViewDist_m); // offset due to left/right eye projection [NB: pretty sure this is a really stupid hack, but it is better than nothing...]
			float2 mouseOffset = (half2(0.5, 0.5)-half2(_MouseX, _MouseY)); // map 0-1 (min/max of screen) to -.5 to +.5
			float4 degradation = tex2D(_Overlay, (i.uv[0].xy - mouseOffset) + offset); //0.13
			float w = 1.0 - saturate(degradation.x*rgb2grey.x + degradation.y*rgb2grey.y + degradation.z*rgb2grey.z);

			// TEST: Invert for debugging
			//w = (1.0 - w);
			//return w;

			// copy over view texture uv coordinates and
			// map the bias term from [0-1] to [0-min_lod] where min_lod is
			// the coarsest mipmap level
			float4 bias_uv = float4(i.uv[0], 0.0, w * _MaxLODlevel - 1); //-1 since w will always be > 0

			// return pixel RGBA info
			return tex2Dlod(_MainTex, bias_uv);
		}
			ENDCG
		}
	}
}
