using UnityEngine;
using System.Collections;

public class fpscounter : MonoBehaviour 
{

// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.
 
public  float updateInterval = 0.5F;
 
private float accum   = 0; // FPS accumulated over the interval
private int   frames  = 0; // Frames drawn over the interval
private float timeleft; // Left time for current interval

private GUIText guiComponent;
	
void Awake() {
   //#if !UNITY_WEBGL
   //Application.targetFrameRate = 60;
   //#endif
   this.useGUILayout = false;
}
 
void Start()
{
	
    if( !GetComponent<GUIText>() )
    {
        Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
        enabled = false;
        return;
			
    }
    guiComponent = GetComponent<GUIText>();
    timeleft = updateInterval; 
}
 
void Update()
{
    timeleft -= Time.deltaTime;
    accum += Time.timeScale/Time.deltaTime;
    ++frames;
    
    // Interval ended - update GUI text and start new interval
    if( timeleft <= 0.0 )
    {
        // display two fractional digits (f2 format)
    float fps = accum/frames;
    string format = System.String.Format("{0:F2} FPS",fps);
    guiComponent.text = format;

    if(fps < 30)
        guiComponent.material.color = Color.white;
    else 
        if(fps < 10)
            guiComponent.material.color = Color.red;
        else
            guiComponent.material.color = Color.red;
    //  DebugConsole.Log(format,level);
        timeleft = updateInterval;
        accum = 0.0F;
        frames = 0;
    }
}
}