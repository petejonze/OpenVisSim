// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*Mips with triplanar and bilinear filtering on downsampled rendertexure.
The Blur system used in the standard package
The Blur optimized used in the standard package
Box filtering
Or any other solution.

https://software.intel.com/en-us/blogs/2014/07/15/an-investigation-of-fast-real-time-gpu-based-image-blur-algorithms
http://forum.unity3d.com/threads/dof-on-ios-how-we-did-it-mini-post-mortem.143887/
http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
*/


Shader "Hidden/myBlur" { // defines the name of the shader 

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		//_Parameter("XXXXX", Vector) = (1, 1, 0, 0)
		//_kernelSize("sdsd", Int) = 1
	}

		CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half4 _MainTex_ST;
		uniform half4 _MainTex_TexelSize;
		uniform half4 _Parameter;
		uniform float _myCurve[100];
		uniform int _kernelSize;

		struct v2f_withBlurCoords
		{
			float4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};

		v2f_withBlurCoords vertBlurHorizontal(appdata_img v)
		{
			v2f_withBlurCoords o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = half4(v.texcoord.xy, 1, 1);
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter.x;
			return o;
		}
		v2f_withBlurCoords vertBlurVertical(appdata_img v)
		{
			v2f_withBlurCoords o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = half4(v.texcoord.xy, 1, 1);
			o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter.x;
			return o;
		}

		half4 fragBlur(v2f_withBlurCoords i) : SV_Target
		{
			half2 uv = i.uv.xy;
			half2 netFilterWidth = i.offs;

			//int k = _kernelSize / 2;
			int k = _kernelSize * 0.5;
			half2 coords = uv - netFilterWidth * k;

			half4 color = 0;
			//for (int l = 0; l < _kernelSize; l++) {
			//	half4 tap = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST));
				// NB: 4th value should be 0 for everything but the original pixel, but here, for simplicity, we shall assume all alpha==1
			//	color += tap * half4(_myCurve[l], _myCurve[l], _myCurve[l], 1);			
			//	coords += netFilterWidth;
			//}
			//GLES (i.e., WebGL) does not support for-loops containing dynamic (non-constant) params in header (_kernelSize). Accordingly, this hack is required:
			for (int l = 0; l < 100; l++) {
				//if (l >= _kernelSize) { break; } // even this won't work, because Unity will optimize the for loop to become a while... which WebGL also doens't allow!!
				if (l < _kernelSize) {
					half4 tap = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST));
					// NB: 4th value should be 0 for everything but the original pixel, but here, for simplicity, we shall assume all alpha==1
					color += tap * half4(_myCurve[l], _myCurve[l], _myCurve[l], 1);
					coords += netFilterWidth;
				} else {
					break;
				}
			}
			return color;
		}
	ENDCG



	SubShader{
		// No culling or depth
		//ZTest Off Cull Off ZWrite Off Blend Off

		// Pass 0: Perform vertical averaging
		Pass{
			ZTest Always
			Cull Off
			CGPROGRAM
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur
			ENDCG
		}

		// Pass 1: Perform horizontal averaging
		Pass{
			ZTest Always
			Cull Off
			CGPROGRAM
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur
			ENDCG
		}

	} // End of SubShader

	FallBack Off
}