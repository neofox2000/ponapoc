using UnityEngine;
using UnityEngine.UI;
using Ponapocalypse;
using GameDB;

public class GUI_TitleScreen : MonoBehaviour 
{
    //public Sprite titleSprite;
    //public Image titleImage;
    public Text versionLabel;

    public AudioManager.TrackRequest musicTrack;

    public void activate()
    {
        versionLabel.text = Constants.gameVersion;
        gameObject.SetActive(true);
        //GUIManager.instance.guiCommon.setBackground(true, Color.white, titleSprite);
        GUIManager.instance.guiCommon.setBackground(true, Color.black);
        GUIManager.instance.guiCommon.mainMenuButtons.Setup(
            GUI_MainMenuButtons.BackgroundMode.TitleScreen,
            GUI_MainMenuButtons.ButtonsMode.Exit);
        GUIManager.instance.guiCommon.OnMainMenu(true);

        //Start music
        AudioManager.instance.PlayMusic(musicTrack);
    }
    public void deactivate()
    {
        //Common.stopMusic(musicToPlay);
        GUIManager.instance.guiCommon.setBackground(true, Color.black);
        GUIManager.instance.guiCommon.OnMainMenu(false);
        gameObject.SetActive(false);
    }
}
