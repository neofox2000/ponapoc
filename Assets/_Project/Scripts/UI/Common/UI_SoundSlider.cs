using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_SoundSlider : MonoBehaviour 
{
    public string key;
    public Slider slider;

    public void OnVolumeChanged(float newValue)
    {
        SendMessageUpwards("OnSliderChanged", this, SendMessageOptions.DontRequireReceiver);
    }
}