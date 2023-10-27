using UnityEngine;
using System.Collections;

public class FogOfWarHidden : MonoBehaviour
{
    Renderer myRenderer;

    void Awake()
    {
        myRenderer = GetComponent<Renderer>();
    }
	void Update ()
	{
        if ((myRenderer) && (FogOfWar.instance))
        {
            myRenderer.enabled = FogOfWar.instance.VisibilityTest(transform.position);
        }
	}
}