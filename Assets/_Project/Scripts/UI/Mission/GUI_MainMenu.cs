using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUI_MainMenu : MonoBehaviour 
{
    public RectTransform upperPanel;

    public void OnDead(bool dead)
    {
        upperPanel.gameObject.SetActive(!dead);
    }
}
