// PJ 13/09/2017
using UnityEngine;

namespace VisSim
{
    public class myWiggle : LinkableBaseEffect
    {
		public enum Algorithm
		{
			Simple,
			Complex
		}

		[Linkable, Tooltip("Animation type. Complex is slower but looks more natural.")]
		public Algorithm Mode = Algorithm.Complex;

		public float Timer = 0f;

        [Linkable, TweakableMember(0.01f, 3f, "Speed", "Wiggle")]
        [Tooltip("Wave animation speed.")]
		public float Speed = 1f;

		[Linkable, Tooltip("Wave frequency (higher means more waves).")]
		public float Frequency = 12f;

        [TweakableMember(0.001f, 0.01f, "Amplitude", "Wiggle")]
        [Linkable, Tooltip("Wave amplitude (higher means bigger waves).")]
		public float Amplitude = 0.01f;

		[Linkable, Tooltip("Automatically animate this effect at runtime.")]
		public bool AutomaticTimer = true;

        protected override void OnUpdate()
		{
			if (AutomaticTimer)
			{
				// Reset the timer after a while, some GPUs don't like big numbers
				if (Timer > 1000f)
					Timer -= 1000f;

				Timer += Speed * Time.deltaTime;
			}
		}

		protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Material.SetVector("_Params", new Vector3(Frequency, Amplitude, Timer * (Mode == Algorithm.Complex ? 0.1f : 1f)));
			Graphics.Blit(source, destination, Material, (int)Mode);
		}

		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myWiggle";
		}
	}
}
