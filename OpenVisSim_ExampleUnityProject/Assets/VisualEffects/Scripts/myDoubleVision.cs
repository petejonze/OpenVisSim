// VisSim
// Copyright (c) 2017 - Pete Jones <petejonze@gmail.com>

// what to do about locked eyes (?) should it be opposite ?

namespace VisSim
{
    using UnityEngine;
    public class myDoubleVision : LinkableBaseEffect
    {

        //[Tooltip("Diploplia strength.")]
        //public Vector2 Displace = new Vector2(0.7f, 0.0f);

        [TweakableMember(-10f, 10f, "X Intensity", "Diplopia")]
        [Linkable, Range(-10f, 10f), Tooltip("X Disparity (prism diopters)")]
        public float DisplaceX_pd = 0.5f;

        [TweakableMember(-10f, 10f, "Y Intensity", "Diplopia")]
        [Linkable, Range(-10f, 10f), Tooltip("Y Disparity (prism diopters)")]
        public float DisplaceY_pd = 10.0f;

        //
        [Linkable, Tooltip("If monocular then displacement will be carried out internally (within each eye).")]
        public bool IsMonocular = false;

        // Monocular-only parameters
        [Linkable, Range(0f, 1f), Tooltip("Blending factor.")]
        public float BlendAmount = 1.0f;

        // geometry info
        [Linkable, Range(100, 10000)]
        public int screenWidth_px = 1334;
        [Linkable, Range(1f, 180.0f)]
        public float viewingAngle_deg = 100.0f;
        private float pixel_per_dg;

        // internal effect parameters
        [Linkable, Tooltip("Diploplia strength.")]
        private Vector2 Displace_px = new Vector2(0.7f, 0.0f);

        protected override void OnDisable()
        {
            this.gameObject.transform.localEulerAngles = new Vector3(0, 0, this.gameObject.transform.localEulerAngles.z);

            base.OnDisable();
        }

        // OnUpdate is called once per frame
        protected override void OnUpdate()
        {
            // if linked then also apply *opposite* offsets to fellow eye
            if (LinkEyes)
            {
                DisplaceX_pd = (isLeftEye ? 1 : -1) * DisplaceX_pd;
                DisplaceY_pd = (isLeftEye ? 1 : -1) * DisplaceY_pd;
            }

            // convert to dioptric blur to rad, then rad to degrees
            float DisplaceX_deg = Mathf.Atan(DisplaceX_pd / 100f) * 180 / Mathf.PI;
            float DisplaceY_deg = Mathf.Atan(DisplaceY_pd / 100f) * 180 / Mathf.PI;

            if (!IsMonocular)
            {
                // set
                this.gameObject.transform.localEulerAngles = new Vector3(DisplaceY_deg, DisplaceX_deg, this.gameObject.transform.localEulerAngles.z);

                // for debugging
                //Debug.Log ("~~~~ " + this.gameObject.transform.localRotation.y + "~~~~ " + this.gameObject.transform.rotation.y + "~~~~ " + this.gameObject.transform.localEulerAngles.y);
            } else {
                this.gameObject.transform.localEulerAngles = new Vector3(0, 0, this.gameObject.transform.localEulerAngles.z);

                // convert deg to pixels
                pixel_per_dg = screenWidth_px / viewingAngle_deg;
                float DisplaceX_px = DisplaceX_deg * pixel_per_dg; // convert deg to pixels
                float DisplaceY_px = DisplaceY_deg * pixel_per_dg; // convert deg to pixels
                Displace_px = new Vector2(DisplaceX_px, DisplaceY_px);
            }
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // If effect is binocular (no special shader required) or zero don't bother applying
            if (!IsMonocular || (Displace_px == Vector2.zero))
            {
                Graphics.Blit(source, destination);
                return;
            }

            // set monocular params and Blit
            Material.SetVector("_Displace", new Vector2(Displace_px.x / (float)source.width, Displace_px.y / (float)source.height));
            Material.SetFloat("_Amount", BlendAmount);
            Graphics.Blit(source, destination, Material);
        }

        protected override string GetShaderName()
        {
            //return "Hidden/VisSim/myDoubleVision";
			return "Hidden/VisSim/BasicShader";
        }
    }
}
