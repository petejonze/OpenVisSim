// PJ 13/09/2017
// TODO: add stereoscopic offset to cursor visualisation (?)
using UnityEngine;

public class GazeTracker : MonoBehaviour
{

    public enum GazeSource
    {
        Fove = 0,
        Tobii = 1,
        Mouse = 2,
        None = 3,
    }
    public GazeSource gazeSource = GazeSource.Mouse;

    public Camera Lcam;
    public Camera Rcam;

    public Vector2 xy_norm = new Vector2(0.5f, 0.5f);
    //public Vector2 xy_px = new Vector2(0.5f, 0.5f);

    // for visualising gaze estimate
    public bool visualiseGaze;
    public Texture2D crosshairImage; // (Texture2D)Resources.Load("crosshair");

    private float prevX = 999f;
    private float prevY = 999f;

    // Singleton
    private static GazeTracker instance; // Singleton instance
    public static GazeTracker GetInstance
    {
        get
        {
            if (instance == null)
            {
                //instance = new GazeTracker();
                //instance = ScriptableObject.CreateInstance("GazeTracker") as GazeTracker;
                //GameObject go = new GameObject();
                //GameObject go = GameObject.Find("Fove Interface");
                //instance = go.AddComponent<GazeTracker>();
                instance = (GazeTracker)FindObjectOfType(typeof(GazeTracker));
            }
            return instance;
        }
    }
    private GazeTracker()
    {
    }
    
    // Use this for initialization
    void Start ()
    {
    }

	// Update is called once per frame
	void Update ()
    {
        switch (gazeSource) {
		case GazeSource.Fove:
                /*
        	// Get convergence data
			Fove.Managed.SFVR_GazeConvergenceData convergence = FoveInterface2.GetFVRHeadset ().GetGazeConvergence ();

	   		// use Ray to get world space coordinate: 
			Vector3 o = new Vector3 (convergence.ray.origin.x, convergence.ray.origin.y, convergence.ray.origin.z);
			Vector3 d = new Vector3 (convergence.ray.direction.x, convergence.ray.direction.y, convergence.ray.direction.z);
			Vector3 pos = o + d * 1.0f;

	     	//.25 to .75? & set
			xy_norm.x = (pos.x + 1f) / 2f;
			xy_norm.y = (pos.y + 1f) / 2f;

          	// finished Fove
			break;
            */
        case GazeSource.Tobii:

                #if !UNITY_WEBGL
                // Get convergence data

                Tobii.Research.Unity.VREyeTracker _eyeTracker = Tobii.Research.Unity.VREyeTracker.Instance;
                if (_eyeTracker == null)
                {
                    Debug.Log("Failed to find eye tracker, has it been added to scene?");
                }

                var latestData = _eyeTracker.LatestGazeData;
                var gazePoint = latestData.CombinedGazeRayWorld.GetPoint(1000f);

                //gazePoint.Normalize();

                
                Vector3 o = new Vector3(0f, 0f, 0f);
                Vector3 d = latestData.CombinedGazeRayWorld.direction;
                Vector3 pos = o + d * 1.0f;

                if (Input.GetKeyDown("space"))
                { 
                    //Debug.Log(latestData.CombinedGazeRayWorld.direction + "    -------     " + latestData.CombinedGazeRayWorld.origin);
                    o = new Vector3(latestData.OriginalGaze.LeftEye.GazeOrigin.PositionInHMDCoordinates.X, latestData.OriginalGaze.LeftEye.GazeOrigin.PositionInHMDCoordinates.Y, latestData.OriginalGaze.LeftEye.GazeOrigin.PositionInHMDCoordinates.Z);
                    d = new Vector3(latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.X, latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.Y, latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.Z);
                    Debug.Log(o + "    -------     " + d);
                }

                if (latestData.CombinedGazeRayWorldValid)
                { 
                d = new Vector3(latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.X, latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.Y, latestData.OriginalGaze.LeftEye.GazeDirection.UnitVector.Z);

                    Vector3 d1 = new Vector3(latestData.OriginalGaze.RightEye.GazeDirection.UnitVector.X, 
                        latestData.OriginalGaze.RightEye.GazeDirection.UnitVector.Y, 
                        latestData.OriginalGaze.RightEye.GazeDirection.UnitVector.Z);
                    d = (d + d1) / 2f;

                    //.25 to .75? & set
                    float tmp = .5f;
                    xy_norm.x = (-d.x * tmp + 0.5f) * 1f + 0.011f;
                    xy_norm.y = (d.y * tmp + 0.5f) * 1f;



                    //NEW
                    if (Lcam != null)
                    {
                        var tmp1 = Lcam.WorldToViewportPoint(gazePoint);
                        xy_norm.x = tmp1.x + 0.022f; // tmp hack!!!
                        xy_norm.y = tmp1.y;

                        var tmp2 = Rcam.WorldToViewportPoint(gazePoint);
                        xy_norm.x = (tmp1.x + tmp2.x) / 2f;
                        xy_norm.y = (tmp1.y + tmp2.y) / 2f;
                    }


                    // only register large movements
                    float dist = Mathf.Sqrt(Mathf.Pow(xy_norm.x - prevX, 2) + Mathf.Pow(xy_norm.y - prevY, 2));
                    //if (dist > 0.01)
                    if (dist > -0.01)
                        {
                        prevX = xy_norm.x;
                        prevY = xy_norm.y;
                    } else
                    {
                        xy_norm.x = prevX;
                        xy_norm.y = prevY;

                    }


                } else
                {
                    //xy_norm.x = 0.55f;
                   // xy_norm.y = 0.55f;

                }

                //Debug.Log(gazePoint +" --> " + xy_norm);

                // finished Tobii

                #endif
                break;

            case GazeSource.Mouse:
         	// Get raw, clip within canvas
			float mousex = Mathf.Min (Mathf.Max (Input.mousePosition.x, 0), Screen.width);
			float mousey = Mathf.Min (Mathf.Max (Input.mousePosition.y, 0), Screen.height);

            // Convert to norm & set
			xy_norm.x = mousex / Screen.width;
			xy_norm.y = mousey / Screen.height;


            #if UNITY_WEBGL && !UNITY_EDITOR
                xy_norm.y = 1 - xy_norm.y;
            #endif

        	// finished Mouse
			break;
		case GazeSource.None:
         	// fix at centre
			xy_norm.x = 0.5f;
			xy_norm.y = 0.5f;

         	// finished None
			break;
		default:
			throw new System.ArgumentException ("Unknown GazeSource parameter?");
		}

        // clamp within 0 to 1 range (defensive)
        xy_norm.x = Mathf.Min(Mathf.Max(xy_norm.x, 0f), 1f);
        xy_norm.y = Mathf.Min(Mathf.Max(xy_norm.y, 0f), 1f);
    }

    void OnGUI()
    {
        if (visualiseGaze)
        { 
            float xMin = (Screen.width / 2)*xy_norm.x - (crosshairImage.width / 2);
            float yMin = Screen.height*(1-xy_norm.y) - (crosshairImage.height / 2);
            GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
            GUI.DrawTexture(new Rect(xMin + (Screen.width / 2), yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
        }
    }
}
