// PJ 13/09/2017
using UnityEngine;

namespace VisSim
{
    public class myLed : LinkableBaseEffect
    {
		public enum SizeMode
		{
			ResolutionIndependent,
			PixelPerfect
		}

        [TweakableMember(1.0f, 100f, "Scale", "z Advanced: Bionic Eye")]
        [Linkable, Range(1f, 100f), Tooltip("Scale of an individual LED. Depends on the Mode used.")]
		public float Scale = 80.0f;

        [TweakableMember(0f, 0.45f, "Margin", "z Advanced: Bionic Eye")]
        [Linkable, Range(0f, 0.45f), Tooltip("Blank margin.")]
        public float Margin = 0.0f;

        [TweakableMember(0f, 10, "Intensity", "z Advanced: Bionic Eye")]
        [Linkable, Range(0f, 10f), Tooltip("LED brightness booster.")]
		public float Brightness = 1.0f;

		[Linkable, Range(1f, 3f), Tooltip("LED shape, from softer to harsher.")]
		public float Shape = 1.5f;

		[Linkable, Tooltip("Turn this on to automatically compute the aspect ratio needed for squared LED.")]
		public bool AutomaticRatio = true;

		[Linkable, Tooltip("Custom aspect ratio.")]
		public float Ratio = 1.0f;

		[Linkable, Tooltip("Used for the Scale field.")]
		public SizeMode Mode = SizeMode.ResolutionIndependent;

        // OnUpdate is called once per frame
        protected override void OnUpdate()
        {
            // must be multiple of 1/scale
            float multipleOf = 1f / Scale;
            int multiple = Mathf.RoundToInt(Margin / multipleOf);
            Margin = multiple * multipleOf;
        }
        
        // Render
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			float scale = Scale;

			if (Mode == SizeMode.PixelPerfect)
				scale = (float)source.width / Scale;

			Material.SetVector("_Params", new Vector4(
					scale,
					AutomaticRatio ? ((float)source.width / (float)source.height) : Ratio,
					Brightness,
					Shape
				));
            Material.SetFloat("_Margin", Margin);

			Graphics.Blit(source, destination, Material);
		}

		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myLed";
		}
	}
}
