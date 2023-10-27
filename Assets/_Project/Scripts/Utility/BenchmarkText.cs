using UnityEngine;
using System.Collections;

/// <summary>
/// Made to find out how many empty game objects will run smoothly
/// </summary>
public class BenchmarkText : MonoBehaviour {

    public int objectsPerFrame = 100;
 
    private int created = 0;
    private bool paused = false;
 
	// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            paused = !paused;

        if (paused) return;

        GameObject go;
        for (int i = 0; i < objectsPerFrame; i++)
        {
            created++;
            //var go = new GameObject();
            go = new GameObject();
            go.transform.parent = transform;
        }
    }
    void OnGUI()
    {
        GUI.TextArea(new Rect(0, 0, 1000, 50), Time.time.ToString("N") + " , " + created);
    }
}