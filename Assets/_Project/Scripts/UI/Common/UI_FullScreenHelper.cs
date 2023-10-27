using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UI_FullScreenHelper : MonoBehaviour 
{
    public float xPos = 1;
    public float yPos = 1;

    RectTransform rTrans;

    void Awake()
    {
        rTrans = GetComponent<RectTransform>();
    }
    void Update()
    {
        if (rTrans)
        {
            //Set max sizes in case screen changed
            rTrans.sizeDelta = new Vector2(Screen.width, Screen.height);

            //Set position based on % vars
            rTrans.anchoredPosition = new Vector2(Screen.width * xPos, Screen.height * yPos);
        }
    }
}
