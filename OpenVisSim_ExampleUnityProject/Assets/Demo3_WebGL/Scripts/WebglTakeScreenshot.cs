using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class WebglTakeScreenshot : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyUp("space"))
        {
            Screenshot();
        }
    }

    // Use this for initialization
    public void Screenshot()
    {
        StartCoroutine(UploadPNG());
        //Debug.log (encodedText);
    }

    IEnumerator UploadPNG()
    {
        // We should only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        //string ToBase64String byte[]
        string encodedText = System.Convert.ToBase64String(bytes);

        var image_url = "data:image/png;base64," + encodedText;

        Debug.Log(image_url);

#if !UNITY_EDITOR
        saveImageToLocal(image_url);
#endif
    }

    [DllImport("__Internal")]
    private static extern void saveImageToLocal(string url);

}