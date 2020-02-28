// PJ 13/09/2017
// TODO: add stereoscopic offset to cursor visualisation (?)
using UnityEngine;

public class GazeTracker : MonoBehaviour
{

    public enum GazeSource
    {
        Fove = 0,
        Mouse = 1,
        None = 2,
    }
    public GazeSource gazeSource = GazeSource.Mouse;

    public Vector2 xy_norm = new Vector2(0.5f, 0.5f);
    //public Vector2 xy_px = new Vector2(0.5f, 0.5f);

    // for visualising gaze estimate
    public bool visualiseGaze;
    private Texture2D crosshairImage;

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
        crosshairImage = (Texture2D)Resources.Load("crosshair");
    }

	// Update is called once per frame
	void Update ()
    {
        switch (gazeSource) {
		case GazeSource.Fove:
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
		case GazeSource.Mouse:
         	// Get raw, clip within canvas
			float mousex = Mathf.Min (Mathf.Max (Input.mousePosition.x, 0), Screen.width);
			float mousey = Mathf.Min (Mathf.Max (Input.mousePosition.y, 0), Screen.height);

            // Convert to norm & set
			xy_norm.x = mousex / Screen.width;
			xy_norm.y = mousey / Screen.height;

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
