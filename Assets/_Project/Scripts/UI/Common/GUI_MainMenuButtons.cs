using UnityEngine;
using UnityEngine.UI;

public class GUI_MainMenuButtons : MonoBehaviour 
{
    public enum BackgroundMode { Normal, TitleScreen }
    public enum ButtonsMode { Exit, Back, Both };

    public Image background;
    public Button
        newButton,
        backButton,
        exitButton;

    public bool isShowing()
    {
        return gameObject.activeSelf;
    }

    void SetBackgroundMode(BackgroundMode value)
    {
        switch (value)
        {
            case BackgroundMode.Normal:
                background.enabled = true;
                break;
            default:
                background.enabled = false;
                break;
        }
    }
    void SetButtonsMode(ButtonsMode value)
    {
        //Enable both buttons by default
        backButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);

        //Switch off the appropriate button if needed
        switch (value)
        {
            case ButtonsMode.Back:
                exitButton.gameObject.SetActive(false);
                break;
            case ButtonsMode.Exit:
                backButton.gameObject.SetActive(false);
                break;
        }
    }
    public void Setup(BackgroundMode backgroundMode, ButtonsMode buttonMode)
    {
        SetBackgroundMode(backgroundMode);
        SetButtonsMode(buttonMode);
    }
}