using UnityEngine;
using System.Collections;
using VisSim;

public class VisSimController : MonoBehaviour
{
    // VR -- Fove or Google VR
    public enum VRType
    {
        Fove = 0,
        GVR = 1
    }
    public VRType vrType = VRType.GVR;
    //private VRType _vrType; // checking for updates -- cannot hotswap
    public GameObject vrGVR;
    private GameObject vrGVRoverlay;
    public GameObject vrFove;

    // Content -- camera feed or static (prerecorded) image
    public enum ContentType
    {
        webcam = 0,
        sphericalImage = 1,
        ovrvision = 2
    }
    public ContentType contentType = ContentType.webcam;
    private ContentType _contentType; // checking for updates
    public GameObject contentWebcam;
    public GameObject contentSphericalImage;

    // key GameObject handles
    public GameObject vfGrabber;
    public GameObject vfEyePicker;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    void OnEnable()
    {
        setVRType(); // set VR presentation method
        setContentType(); // set content method
    }

    void Update()
    {
        // if (_vrType != vrType) { setVRType(); } // cannot change VR type once loaded
        if (_contentType != contentType) { setContentType(); }
    }

    private void setVRType()
    {
        vrGVR.SetActive(vrType == VRType.GVR);
        vrFove.SetActive(vrType == VRType.Fove);

        if (vrType == VRType.GVR)
        {
            vrGVRoverlay = GameObject.Find("GvrViewer");
        }
    }

    
    public GameObject getVRgameObject()
    {
        switch (vrType)
        {
            case VRType.GVR:
                return vrGVR;
            case VRType.Fove:
                return vrFove;
            default:
                Debug.LogError("Unrecognised VR type???");
                return null;
        }
    }
    

    private void setContentType()
    {
        contentWebcam.SetActive(contentType == ContentType.webcam);
        contentSphericalImage.SetActive(contentType == ContentType.sphericalImage);
        _contentType = contentType;
    }


    public GameObject getContentgameObject()
    {
        switch (contentType)
        {
            case ContentType.webcam:
                return contentWebcam;
            case ContentType.sphericalImage:
                return contentSphericalImage;
            default:
                Debug.LogError("Unrecognised Content type???");
                return null;
        }
    }


    public void showMainScreen()
    {
        Debug.Log("VisSimController: **Show Main Screen**");
        getVRgameObject().SetActive(true);
        vfEyePicker.SetActive(true);
        vfGrabber.SetActive(false);
        getContentgameObject().SetActive(true);
        if (contentType == ContentType.webcam)
        {
            contentWebcam.GetComponent<DeviceCameraController>().StartWork();
        }

        // Hack? for some reason FoveRig disables the individual left/right eye cameras. This doesn't initially appear to be a problem, but means that if the rig itself is disable/re-enabled, nothing happens...
        if (vrType == VRType.Fove)
        {
            Component[] cams = vrFove.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cams)
            {
                cam.enabled = true;
            }
        }

        // Hack? for some reason GVR creates a secondary overlay
        if (vrType == VRType.GVR)
        {
            vrGVRoverlay.SetActive(true);
        }
    }

    public void hideMainScreen()
    {
        Debug.Log("VisSimController: **HIDE Main Screen**");
        //contentWebcam.SetActive(false);
        //contentSphericalImage.SetActive(false);
        if (contentType == ContentType.webcam)
        {
            contentWebcam.GetComponent<DeviceCameraController>().StopWork();
        }
        getContentgameObject().SetActive(false);
        getVRgameObject().SetActive(false);
        vfEyePicker.SetActive(false);
        vfGrabber.SetActive(true);

        // Hack? for some reason GVR creates a secondary overlay
        if (vrType == VRType.GVR)
        {
            vrGVRoverlay.SetActive(false);
        }
    }

    // Singleton
    private static VisSimController instance; // Singleton instance
    public static VisSimController GetInstance
    {
        get
        {
            if (instance == null)
            {
                instance = (VisSimController)FindObjectOfType(typeof(VisSimController));
            }
            return instance;
        }
    }
    private VisSimController()
    {
    }
}
