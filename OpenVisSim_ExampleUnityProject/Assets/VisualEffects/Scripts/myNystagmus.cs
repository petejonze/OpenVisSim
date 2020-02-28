// PJ
// TODO - make nulling map alterable
// TODO - make compatible with Double Vision script (currently not!)
// TODO - artificial/monocular is untested and likely broken

using UnityEngine;

namespace VisSim
{
    public class myNystagmus : LinkableBaseEffect
     {
        [Linkable, TweakableMember(0f, 1f, "Foveation dur (secs)", "Nystagmus")]
        public float foveat_d = 0.12f;                  // duration of sustained/'0'/foveation period [0.12 in C & B]
        [Linkable, TweakableMember(0f, 1f, "Rise dur (secs)", "Nystagmus")]
        public float rise_d = 0.13f;                    // duration of rise period [~0.15 in C & B]
        [Linkable, TweakableMember(0.1f, 4f, "Rise exponent", "Nystagmus")]
        public float rise_exp = 1.75f;        	        // rise exponent(e.g, 1==linear, 2==exponential)  [~1.75 in C&B]
        [Linkable, TweakableMember(0.1f, 20f, "Ampltidue (degs)", "Nystagmus")]
        public float amp_deg = 8f;                      // total shift rightwards [8 in C & B]
        [Linkable, TweakableMember(0.1f, 4f, "Return Error (degs)", "Nystagmus")]
        public float baselineErr_deg = 1f / 60f * 44f;  // uniform random error in baseline(e.g., 0 to 0.7333) [0.7333 in C&B]
        [Linkable, Range(0f,359.9f), TweakableMember(0f, 359.9f, "Angle", "Nystagmus")]
        public float direction_deg = 0f;  // direction of travel (currently only used in artificial/monocular mode)

        // binocular (rotate camera) or monocular (transpose image)
        [Linkable, Tooltip("If artificialRotation=true then displacement will be carried out by shifting/extrapolating pixels, rather than by rotating the camera (appropriate for 2D images).")]
        public bool artificialRotation = false;

        // nulling map
        [Linkable, TweakableMember(0, 1, "Use Nulling", "Nystagmus")]
        public bool useNullingField = true;  // uniform random error in baseline(e.g., 0 to 0.7333) [0.7333 in C&B]
        [Linkable]
        public Texture2D nullTex;
        [SerializeField]
        private float _nullAmount;
        public float nullAmount { get { return _nullAmount; } }

        // internal params
        private float baselineShift_deg = 0f;
        private float timer_secs = 0f;
        private float t_cycle = 0f;
        private float t_cycle_prev = 0f;
        private float shift_deg = 0f;
        private float old_shift_deg = 0f;
        [Tooltip("Diploplia strength.")]
        private Vector2 Displace = new Vector2(0.7f, 0.0f);
        
        // geometry info
        [Linkable, Range(100, 10000), Tooltip("Only required if using artificial rotation")]
        public int screenWidth_px = 1334;
        [Linkable, Range(1f, 180.0f), Tooltip("Only required if using artificial rotation")]
        public float viewingAngle_deg = 100.0f;
        private float pixel_per_dg;

        // init
        protected new void OnEnable()
        {
            base.OnEnable();

            // set overlay texture
            nullTex = (Texture2D)Resources.Load("nystagmus_nullingmap2_512", typeof(Texture2D));
            Material.SetTexture("_NullingOverlay", nullTex);
        }

        protected new void OnDisable()
        {
            // remove any existing shift
            this.gameObject.transform.localEulerAngles = new Vector3(this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y-old_shift_deg, this.gameObject.transform.localEulerAngles.z);

            base.OnDisable();
        }

        protected override void OnUpdate()
        {
            pixel_per_dg = screenWidth_px / viewingAngle_deg;

            //Update timer
            timer_secs += Time.deltaTime;
            // Reset the timer after a while, some GPUs don't like big numbers
            if (timer_secs > 1000f)
            {
                timer_secs -= 1000f;
            }
            
            // Gaze-contingent:
            // Get
            Vector2 xy_norm = GazeTracker.GetInstance.xy_norm;
            float x = xy_norm.x;
            float y = xy_norm.y;
            // Adjust
            //x = (x - 0.3f) / (0.7f - 0.3f); // TMP HACK!!! (to ensure actually use full range of 0 to 1, remap from ~0.3 to 7)
            //y = (y - 0.3f) / (0.7f - 0.3f); // TMP HACK!!! (to ensure actually use full range of 0 to 1, remap from ~0.3 to 7)
            // Set
            x = Mathf.Min(Mathf.Max(x, 0f), 1f);
            y = Mathf.Min(Mathf.Max(y, 0f), 1f);

            // Debugging
            //Debug.Log ("x=" + x + ",  y=" + y);


            /*------ Simulate jerk nystagmus ------ */
            // get time elapsed within current period
            float total_d = foveat_d + rise_d;
            t_cycle = timer_secs % total_d;

            // if starting a new peiod, generate a new random baseline
            if (t_cycle < t_cycle_prev)
            {
                // reset camera
                float starting_deg = this.gameObject.transform.localEulerAngles.y - old_shift_deg;
                this.gameObject.transform.localEulerAngles = new Vector3(this.gameObject.transform.localEulerAngles.x, starting_deg, this.gameObject.transform.localEulerAngles.z);
                old_shift_deg = starting_deg;
                
                baselineShift_deg = Random.Range(0f, baselineErr_deg) - starting_deg;
            }
            t_cycle_prev = t_cycle;

            // get x value, between 0 and 1 (proportion of the way through the current period)
            float shift_proportion;
            if (t_cycle <= foveat_d)
            {
                shift_proportion = 0;
            } else {
                shift_proportion = (t_cycle - foveat_d) / rise_d; // proportion of rise period
                shift_proportion = Mathf.Pow(shift_proportion, rise_exp);
            }

            // apply amplitude(scale x to be: 0 <= x <= amp_deg)
            shift_deg = shift_proportion * amp_deg;

            // apply baseline(scale x to be: baseline_deg <= x <= amp_deg)
            shift_deg = Mathf.Max(shift_deg, baselineShift_deg);

            if (!artificialRotation) // rotate camera(s)
            {
                if (useNullingField)
                {
                    Color nulling = nullTex.GetPixelBilinear(x, y);
                    _nullAmount = (nulling.r + nulling.g + nulling.b) / 3f;
                    shift_deg *= (1 - Mathf.Sqrt(_nullAmount)); // squareroot to increase the nulling effect
                }
                // compute delta
                float shift_delta_deg = shift_deg - old_shift_deg;
                old_shift_deg = shift_deg;
                // set
                //transform.Rotate(Vector3.up * shift_delta_deg);
                this.gameObject.transform.localEulerAngles = new Vector3(this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y+shift_delta_deg, this.gameObject.transform.localEulerAngles.z);
            } else {
			    Material.SetFloat("_MouseX", x); 
			    Material.SetFloat("_MouseY", y);
            }
        }

        private float deg2rad = Mathf.PI / 180f;
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!artificialRotation) // rotate camera(s)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // artificial rotation
            //Displace = new Vector2(shift_deg * pixel_per_dg, 0f); // force horizontal
            float xOffset_deg = -shift_deg * Mathf.Cos(direction_deg * deg2rad); //???
            float yOffset_deg = shift_deg * Mathf.Sin(direction_deg * deg2rad);
            Displace = new Vector2(xOffset_deg * pixel_per_dg, yOffset_deg * pixel_per_dg);
            if (Displace == Vector2.zero)
            {
                Graphics.Blit(source, destination);
                return;
            }
            // debugging:
            //Debug.Log(Displace.x + ", " + Displace.y);
            Material.SetVector("_Displace", new Vector2(Displace.x / (float)source.width, Displace.y / (float)source.height));
            Graphics.Blit(source, destination, Material, useNullingField ? 1 : 0);
        }

				
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myNystagmus";
		}
    }
}
