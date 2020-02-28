/*
 * Author : Maxime JUMELLE
 * Namespace : AcidTrip
 * Project : AcidTrip
 * 
 * If you have any suggestion or comment, you can write me at webmaster[at]hardgames3d.com
 * 
 * File : AcidTrip.cs
 * Abstract : This is the main script that allows you to creates an acid trip effect on camera.
 * 
 * */

using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace AcidTrip
{
	[ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	[AddComponentMenu ("Image Effects/Acid Trip")]
	public class AcidTrip : PostEffectsBase
	{

		private float timer = 0;

        private const String tweaklabel = "z Advanced: Complex Haze";
        [TweakableMember(0.0f, 10.0f, "wavelength", tweaklabel)]
        [Range(0.0f, 10.0f)]
        public float Wavelength = 1.0f;
        [TweakableMember(0.0f, 3.0f, "strength", tweaklabel)]
        [Range(0.0f, 3f)]
        public float DistortionStrength = 0.25f;
        [TweakableMember(0, 1, "sparkling", tweaklabel)]
        public bool Sparkling = false;
        [TweakableMember(0.1f, 10f, "base", tweaklabel)]
        [Range(0.1f, 10.0f)]
        public float SaturationBase = 1.0f;
        [TweakableMember(0f, 10f, "speed", tweaklabel)]
        [Range(0.0f, 10.0f)]
        public float SaturationSpeed = 1.0f;
        [TweakableMember(0.1f, 10f, "amplitude", tweaklabel)]
        [Range(0.1f, 10.0f)]
        public float SaturationAmplitude = 0.3f;
		
		public Shader currentShader = null;
		private Material currentMaterial = null;
		
		public override bool CheckResources ()
		{
			currentShader = Shader.Find ("AcidTrip/AcidTrip");
			CheckSupport (false);
			currentMaterial = CheckShaderAndCreateMaterial(currentShader, currentMaterial);
			
			if (!isSupported)
				ReportAutoDisable ();
			return isSupported;
		}
		
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			timer += Time.deltaTime;

			currentMaterial.SetFloat ("timer", timer);
			currentMaterial.SetFloat ("speed", 1);
			currentMaterial.SetFloat ("distortion", 0.25f);
			currentMaterial.SetFloat ("amplitude", 70.0f);
			currentMaterial.SetFloat ("satbase", SaturationBase);
			currentMaterial.SetFloat ("satSpeed", SaturationSpeed);
			currentMaterial.SetFloat ("satAmp", SaturationAmplitude);
			currentMaterial.SetFloat ("strength", Wavelength);
			currentMaterial.SetFloat ("distortion", DistortionStrength);
			currentMaterial.SetInt ("sparkling", (Sparkling) ? 1 : 0);

			if (!CheckResources())
			{
				Graphics.Blit (source, destination);
				return;
			}
			Graphics.Blit (source, destination, currentMaterial);
		}
	}
	
}
