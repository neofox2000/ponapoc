using UnityEngine;
using System.IO;
using System.Collections;

public class ScreenShotter : MonoBehaviour 
{
    public int scale = 1;
    public bool usePersistentDataPath = false;
    public string customPath = "C:/";
    public KeyCode button = KeyCode.F12;

	void Update () 
    {
        if (InputX.Up(button))
        {
            string path;

#if UNITY_EDITOR
            if (!usePersistentDataPath)
                path = customPath;
            else
#endif
            path = Application.persistentDataPath + "/Screenshots/";

            string dt = System.DateTime.Now.ToString().Replace('/', '_').Replace(':', '_');
            string filename = path + "cap " + dt + ".png";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ScreenCapture.CaptureScreenshot(filename, scale);

            string msg = "Screenshot saved to: " + filename;
            StartCoroutine(DelayedAlert(msg));
        }
	}

    IEnumerator DelayedAlert(string msg)
    {
        yield return new WaitForEndOfFrame();

        Alerter.ShowMessage(msg, 5f);
    }
}