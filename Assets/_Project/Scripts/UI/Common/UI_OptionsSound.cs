using UnityEngine;
using UnityEngine.Audio;

public class UI_OptionsSound : MonoBehaviour 
{
    const string closeMessage = "OnMain";

    public GameSettings settings;
    public UI_SoundSlider[] volumeSliders;
    bool changed = false;

    void OnSliderChanged(UI_SoundSlider slider)
    {
        Common.setMixerVolume(settings.masterMixer, slider.key, slider.slider.value);
        changed = true;
    }
    void close()
    {
        SendMessageUpwards(closeMessage, SendMessageOptions.DontRequireReceiver);
    }
    
    public void OnShow()
    {
        foreach (UI_SoundSlider ss in volumeSliders)
            ss.slider.value = Common.getMixerVolume(settings.masterMixer, ss.key);

        changed = false;
    }
    public void OnLoad()
    {
        settings.Load();
        OnShow();
    }
    public void OnSave()
    {
        if (changed)
        {
            foreach (UI_SoundSlider ss in volumeSliders)
                settings.SetKey(ss.key, ss.slider.value);

            changed = false;
        }

        settings.Save();
        close();
    }
    public void OnCancel()
    {
        if (changed)
            OnLoad();

        close();
    }
}