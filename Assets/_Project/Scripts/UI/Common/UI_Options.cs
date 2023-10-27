using UnityEngine;

public class UI_Options : MonoBehaviour 
{
    const string closeMessage = "OnOptions";
    public enum Menus { Hidden = -1, Main = 0, Sound = 1, Graphics = 2, Controls = 3, Gameplay = 4 };

    public UI_OptionsSound soundOptions;
    public UI_OptionsGameplay gameplayOptions;
    public RectTransform
        mainPanel,
        soundPanel,
        graphicsPanel,
        controlsPanel,
        gameplayPanel;

    Menus _currentMenu = Menus.Main;
    public Menus currentMenu
    {
        get { return _currentMenu; }
        set
        {
            setCurrentMenuVisibility(false);
            _currentMenu = value;
            setCurrentMenuVisibility(true);

            if (value == Menus.Hidden)
                SendMessageUpwards(closeMessage, false, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void OnEnable()
    {
        OnMain();
    }
    
    void setCurrentMenuVisibility(bool showIt)
    {
        switch (currentMenu)
        {
            case Menus.Main:
                mainPanel.gameObject.SetActive(showIt);
                break;
            case Menus.Sound:
                soundPanel.gameObject.SetActive(showIt);
                break;
            case Menus.Graphics:
                graphicsPanel.gameObject.SetActive(showIt);
                break;
            case Menus.Controls:
                controlsPanel.gameObject.SetActive(showIt);
                break;
            case Menus.Gameplay:
                gameplayPanel.gameObject.SetActive(showIt);
                break;
        }
    }
    public void OnMain()
    {
        currentMenu = Menus.Main;
    }
    public void OnSoundOptions()
    {
        currentMenu = Menus.Sound;
        soundOptions.OnLoad();
    }
    public void OnGameplayOptions()
    {
        currentMenu = Menus.Gameplay;
        gameplayOptions.OnShow();
    }
    public void OnClose()
    {
        currentMenu = Menus.Hidden;
    }
}
