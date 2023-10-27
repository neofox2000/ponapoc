using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugTextOutput : MonoBehaviour 
{
    public Text textObject;

	void FixedUpdate() 
    {
        if(textObject)
            textObject.text = "";
	}
}
