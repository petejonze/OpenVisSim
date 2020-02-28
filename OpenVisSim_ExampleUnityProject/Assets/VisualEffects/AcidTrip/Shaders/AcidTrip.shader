// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Author : Maxime JUMELLE
// Project : AcidTrip
// If you have any suggestion or comment, you can write me at webmaster[at]hardgames3d.com

Shader "AcidTrip/AcidTrip" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	float strength = 1.0f;
	int sparkling = 0;
	float timer, speed = 1;
	float amplitude = 70, distortion = 0.25f;
	float sat;
	
	float satbase, satSpeed, satAmp;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	} 
	
	float4 frag(v2f i) : SV_Target 
	{
		float3 satWeight = float3(0.299f, 0.587f, 0.114f);
		float2 texCoords = i.uv;
		texCoords.x += sin(timer * speed + texCoords.x * amplitude * strength) * distortion * 0.01f;
	    texCoords.y += cos(timer * speed + texCoords.y * amplitude * strength) * distortion * 0.01f;
	    
		float4 result = tex2D(_MainTex, texCoords);
		  
		result += tex2D(_MainTex, texCoords);
		result -= tex2D(_MainTex, texCoords);
		result += tex2D(_MainTex, texCoords);
		result -= tex2D(_MainTex, texCoords);
		
		result += tex2D(_MainTex, texCoords);
		float luminance = dot(result, satWeight);
		sat = satbase + cos(timer * satSpeed * 0.4f) * satAmp;
		result = lerp(luminance, result, float4(1.1f + sat, 0.7f, 0.2f + sat * 0.8f, 0.2f));
		result += tex2D(_MainTex, texCoords);
		
		if (sparkling)
		{
			float factor = 0.5f + 0.1f * (cos(timer * 0.5f) + sin(timer * 0.5f));
			
			result.rgb = pow(result, float3(factor, factor, factor));
			result.rgb *= 20 * cos(timer) + 50;
			result.rgb = floor(result);
			result.rgb /= 20 * cos(timer) + 50;
			result.rgb = pow(result, (float3)(1 / factor));
		}
		
		result += tex2D(_MainTex, texCoords);
		result += tex2D(_MainTex, texCoords);
		luminance = dot(result, satWeight);
		result = lerp(luminance, result, float4(1.0f, sat + 0.2f, sat + 0.3f, 1.0f));
		result += tex2D(_MainTex, texCoords);
		result -= tex2D(_MainTex, i.uv);
		result -= tex2D(_MainTex, i.uv);
		result += tex2D(_MainTex, i.uv);
		
		
		 
		result = result / 5;	
		return result;
	}

	ENDCG 
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
  
}

Fallback off
	
}

