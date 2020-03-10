/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class DeviceCameraController : MonoBehaviour {

	public enum CameraMode
	{
		FACE_C,
		DEFAULT_C,
		NONE
	}

	[HideInInspector]
	public WebCamTexture cameraTexture; 

	private bool isPlay = false;
	GameObject e_CameraPlaneObj;
	bool isCorrected = false;
	float screenVideoRatio = 1.0f;
	public bool isPlaying
	{
		get{
			return isPlay;
		}
	}

	// Use this for initialization  
	void Awake()  
	{
        StartWork();
    }

	// Update is called once per frame  
	void Update()  
	{  
		if (isPlay) {  
			if(e_CameraPlaneObj.activeSelf)
			{
				e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = cameraTexture;
			}
		}
        
		if (cameraTexture != null && cameraTexture.isPlaying) {
			if (cameraTexture.width > 200 && !isCorrected) {
				correctScreenRatio();
			}
		}

	}

	IEnumerator CamCon()  
	{  
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);  
		if (Application.HasUserAuthorization(UserAuthorization.WebCam))  
		{  
			#if UNITY_EDITOR_WIN || UNITY_STANDALONE
			cameraTexture = new WebCamTexture();  
			#elif UNITY_EDITOR_OSX
			cameraTexture = new WebCamTexture(640,480);  
			#elif UNITY_IOS
			cameraTexture = new WebCamTexture(640,480);
			#elif UNITY_ANDROID
			cameraTexture = new WebCamTexture(640,480);
			#else
			cameraTexture = new WebCamTexture(); 
			#endif

			cameraTexture.Play();
			isPlay = true;  
		}  
	}

	/// <summary>
	/// Stops the work.
	/// when you need to leave current scene ,you must call this func firstly
	/// </summary>
	public void StopWork()
	{
		isPlay = false;
		if (this.cameraTexture != null && this.cameraTexture.isPlaying) {
			this.cameraTexture.Stop();
			Destroy(this.cameraTexture);
			this.cameraTexture = null;
		}
	}

    public void StartWork()
    {
        StartCoroutine(CamCon());
        e_CameraPlaneObj = transform.Find("CameraPlane").gameObject;
    }

    /// <summary>
    /// Corrects the screen ratio.
    /// </summary>
    void correctScreenRatio()
	{
		int videoWidth = 1;
		int videoHeight = 1;
			int ScreenWidth = 1;
			int ScreenHeight = 1;

		float videoRatio = 1;
		float screenRatio = 1;

		if (cameraTexture != null) {
			videoWidth = cameraTexture.width;
			videoHeight = cameraTexture.height;
		}
		videoRatio = videoWidth * 1.0f / videoHeight;
		ScreenWidth = Mathf.Max (Screen.width, Screen.height);
		ScreenHeight = Mathf.Min (Screen.width, Screen.height);
		screenRatio = ScreenWidth * 1.0f / ScreenHeight;

		screenVideoRatio = screenRatio / videoRatio;
		isCorrected = true;

		if (e_CameraPlaneObj != null) {
			e_CameraPlaneObj.GetComponent<CameraPlaneController>().correctPlaneScale(screenVideoRatio);
			//e_CameraPlaneObj_right.GetComponent<CameraPlaneController>().correctPlaneScale(screenVideoRatio);
		}
	}

	public float getScreenVideoRatio()
	{
		return screenVideoRatio;
	}

}