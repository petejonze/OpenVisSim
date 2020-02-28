// PJ 13/09/2017

Shader "Hidden/VisSim/myRecolour"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		//NB: _ColourTransform no longer listed in Properties, as Matrix data type not supported. Listed inside Pass instead
		//_ColourTransform ("dfdfdf", Matrix) = ((1,0,0,0),(0,1,0,0),(0,0,1,0),(0,0,0,1))
	}
	SubShader
	{
		// No depth -- NB: disabled as doesn't seem to be an effective optimization
		//ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			// define functions
			#pragma vertex vert_img // Tells the cg to use default vertex-shader
			#pragma fragment frag // Tells the cg to use a fragment-shader called frab
			#pragma fragmentoption ARB_precision_hint_fastest // versus ARB_precision_hint_nicest

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4x4 _ColourTransform; //NB: not listed in Properties, as Matrix data type not supported. But still works(!)
			half4 _MainTex_ST;

			fixed4 frag (v2f_img i) : SV_Target
			{	
				// Get texture fragment at this location
				half4 original = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));

				// Correct for gamma (??) -- NB: apparently, not required, according to authors (XXXXXX)
				//original.xyzw = pow(original.xyzw, 2.2);
				//original.xyz = GammaToLinearSpace(original.xyz); // ALT

				// Transform
				half4 output = mul(_ColourTransform, original.xyzw);

				// Uncorrect for gamma (??)
				//output.xyzw = pow(output.xyzw, 1/2.2);			
				//output.xyz = LinearToGammaSpace(output.xyz); // ALT

				// sometimes,the transformed colour is outside the device gamut (0 to 1). It this case it is correct to  round the value to the closest value within the gamut
				output.xyzw = clamp(output.xyzw, 0, 1);

				// Return
				return output;
			}

			ENDCG
		}
	}
}
