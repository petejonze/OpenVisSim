using System;
using System.Collections;
using UnityEngine;

// AKA 'scintillating scotoma' or 'visual migraine'
namespace VisSim
{
    
    public class myTeichopsia : LinkableBaseEffect
    {
        [Linkable, Range(0f, 1f), Tooltip("Strength used to apply the noise. 0 means no noise at all, 1 is full noise."), TweakableMember(0.0f, 1.0f, "Intensity", "Teichopsia")]
        public float Strength = 0.75f;
        
        [Linkable, Range(0f, 1f), Tooltip("Reduce the noise visibility in luminous areas."), TweakableMember(0.0f, 1.0f, "Luminance", "Teichopsia")]
        public float LumContribution = 0.75f;

        [Linkable, Tooltip("Whether or not to follow the point of gaze."), TweakableMember(0.0f, 1.0f, "Gaze Contingent", "Teichopsia")]
        public bool gazeContingent = true;

        // internal
        private float Seed = 0.5f; //  number used to initialize the noise generator
        private int width_px = 512;
        private int height_px = 512;
        // outer/overall properties
        private int numOfPoints = 30;
        private float maxRadius = 150f;
        private float spikiness = 0.75f; // 0 to 1
        // inner properties
        int widthMu_px = 70;
        int widthStd_px = 15;
        int minRadius_px = 30;

        public new void OnEnable()
        {
            // 
            base.OnEnable();

            // Create random polygons
            Vector2[] outerPolygon = CreatePoly(numOfPoints, maxRadius, spikiness); 
            Vector2[] innerPolygon = CreateInnerPoly(outerPolygon, widthMu_px, widthStd_px, minRadius_px);
            // (re)position both at specified centroid
            for (int i = 0; i < numOfPoints; i++)
            {
                outerPolygon[i].x += 256;
                outerPolygon[i].y += 256;
                innerPolygon[i].x += 256;
                innerPolygon[i].y += 256;
            }

            // Create texture
            Color[] px = new Color[width_px * height_px];
            Vector2 currentPoint;
            for (int x = 0; x < width_px; x++)
            {
                for (int y = 0; y < height_px; y++)
                {
                    currentPoint = new Vector2(x, y);
                    if (IsPointInPolygon(currentPoint, outerPolygon) && !IsPointInPolygon(currentPoint, innerPolygon))
                    {
                        px[x + y * height_px] = Color.white;
                    }
                    else
                    {
                        px[x + y * height_px] = Color.black;
                    }
                }
            }
            Texture2D tex = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            tex.SetPixels(px);
            tex.Apply(false); // actually apply all SetPixels, don't recalculate mip levels

            // for debugging: draw texture to screen!
            //GameObject currentLoc = GameObject.Find("Cube (Street Scene)"); //("Cube (Texture2D)");
            //currentLoc.GetComponent<Renderer>().material.mainTexture = tex; // offsetTextureX;

            // set as mask overlay in shader
            Material.SetTexture("_MaskOverlay", tex);
        }

        // Update is called once per frame
		protected override void OnUpdate()
        {
            // Update seed
            Seed += Time.deltaTime * 0.25f;
            // Reset the Seed after a while, some GPUs don't like big numbers
            if (Seed > 1000f)
                Seed = 0.5f;
            
            // Gaze-contingent
            if (gazeContingent)
            {
                Vector2 xy_norm = GazeTracker.GetInstance.xy_norm;
                Material.SetFloat("_MouseX", 1 - xy_norm.x); // Why 1-x???
                Material.SetFloat("_MouseY", xy_norm.y);
            }
            else // if not gaze contingent then centralise
            {
                Material.SetFloat("_MouseX", 0.5f);
                Material.SetFloat("_MouseY", 0.5f);
            }
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // update params
            Material.SetVector("_Params", new Vector3(Seed, Strength, LumContribution));

            // Blit
            Graphics.Blit(source, destination, Material);
        }




        // -----------------------------------------------------------------------------
        // GENERATING TEICHOPSIA OVERLAY TEXTURE
        // -----------------------------------------------------------------------------

        private Vector2[] CreatePoly(int numOfPoints, float maxRadius, float spikiness)
        {
            // init output
            Vector2[] polygon = new Vector2[numOfPoints];

            float theta, rho, x, y;
            for (int i = 0; i < numOfPoints; i++)
            {

                theta = (Mathf.PI * 2) / (numOfPoints + 1) * i;

                if (spikiness == 0)
                {
                    rho = maxRadius;
                }
                else
                {
                    rho = (maxRadius * (1 - spikiness)) + UnityEngine.Random.Range(0, (int)(maxRadius * spikiness));
                }

                // Polar to Cartesian
                x = rho * Mathf.Cos(theta);
                y = rho * Mathf.Sin(theta);
                polygon[i] = new Vector2(x, y);
            }

            return polygon;
        }

        private Vector2[] CreateInnerPoly(Vector2[] polygon, float width_mu, float width_std, float minRadius_px)
        {
            // init output
            int numOfPoints = polygon.Length;
            Vector2[] innerPolygon = new Vector2[numOfPoints];
            int minpixels = 10;

            for (int i = 0; i < numOfPoints; i++)
            {
                // generate a random number
                float r = Mathf.Max(generateNormalRandom(width_mu, width_std), minpixels); // can't be less than minpixels

                // subtract from outer point (converting from cartesian to polar)
                float outer_dist = Mathf.Sqrt(Mathf.Pow(polygon[i].x, 2) + Mathf.Pow(polygon[i].y, 2));
                float outer_angle = Mathf.Atan2(polygon[i].y, polygon[i].x);
                float inner_dist = Mathf.Max(outer_dist - r, minRadius_px);

                // convert back from polar to cartesian
                float x = inner_dist * Mathf.Cos(outer_angle);
                float y = inner_dist * Mathf.Sin(outer_angle);

                // store
                innerPolygon[i] = new Vector2(x, y);
            }
            return innerPolygon;
        }

        private static float generateNormalRandom(float mu, float sigma)
        {
            float rand1 = UnityEngine.Random.Range(0.0f, 1.0f);
            float rand2 = UnityEngine.Random.Range(0.0f, 1.0f);

            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

            return (mu + sigma * n);
        }

        private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            int polygonLength = polygon.Length, i = 0;
            bool inside = false;
            // x, y for tested point.
            float pointX = point.x, pointY = point.y;
            // start / end point for the current polygon segment.
            float startX, startY, endX, endY;
            Vector2 endPoint = polygon[polygonLength - 1];
            endX = endPoint.x;
            endY = endPoint.y;
            while (i < polygonLength)
            {
                startX = endX; startY = endY;
                endPoint = polygon[i++];
                endX = endPoint.x; endY = endPoint.y;
                //
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }
            return inside;
        }
		
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myScintillate";
		}
    }
}
