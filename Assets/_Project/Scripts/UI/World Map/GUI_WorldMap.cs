using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using GameDB;

public class GUI_WorldMap : MonoBehaviour 
{
    const float missionStartDelay = 2f;
    WorldMapOverlayStates currentState
    {
        get
        {
            return (WorldMapOverlayStates)animator.GetInteger(akhState);
        }
        set
        {
            animator.SetInteger(akhState, (int)value);
        }
    }

    public AudioManager.TrackRequest musicToPlay;
    public AudioGroupTemplate missionStartSound;
    public enum WorldMapOverlayStates { Idle = 0, AtLocation = 1 };

    public GUI_LocationPanel guiLocationPanel;
    public HUD_Bar HPBar, SPBar;
    public UnityEvent OnMapAreaPointerEnter = new UnityEvent();
    public UnityEvent OnMapAreaPointerExit = new UnityEvent();
    public UnityEvent OnCenterMap = new UnityEvent();

    int akhState;
    Animator animator;

    public void OnEnterLocation(WorldLocationEntrance entrance)
    {
        AudioManager.instance.Play(missionStartSound);
        GUIManager.instance.guiCommon.OnFadeOut(true, missionStartDelay);
        StartCoroutine(EnterLocation(missionStartDelay, entrance));
    }
    IEnumerator EnterLocation(float startDelay, WorldLocationEntrance entrance)
    {
        yield return new WaitForSeconds(startDelay);
        GameDatabase.metaGame.StartMission(entrance);
    }
    public void OnMenuButtonClick()
    {
        GUIManager.instance.guiCommon.OnToggleButtons();
    }
    public void show()
    {
        currentState = WorldMapOverlayStates.AtLocation;
    }
    public void hide()
    {
        currentState = WorldMapOverlayStates.Idle;
    }

    //Event Handlers
    public void mapAreaPointerEnter()
    {
        OnMapAreaPointerEnter.Invoke();
    }
    public void mapAreaPointerExit()
    {
        OnMapAreaPointerExit.Invoke();
    }
    public void centerMap()
    {
        OnCenterMap.Invoke();
    }

    //Init/deinit
    public void activate()
    {
        //Update game state
        GameDatabase.metaGame.currentState = Metagame.MetaGameStates.Map;

        GUIManager.instance.guiCommon.setBackground(false, Color.black);
        GUIManager.instance.guiCommon.mainMenuButtons.Setup(
            GUI_MainMenuButtons.BackgroundMode.Normal,
            GUI_MainMenuButtons.ButtonsMode.Both);

        //Cache stuff
        if (!animator)
        {
            animator = GetComponent<Animator>();
            akhState = Animator.StringToHash("State");
        }

        //Play music
        AudioManager.instance.PlayMusic(musicToPlay);

        //Setup Player's stat readouts
        CharacterSheet characterSheet = GameDatabase.sPlayerData.characterSheet;
        if (characterSheet != null)
        {
            GameDatabase.sPlayerData.characterSheet.CalcStats();
            HPBar.SetAttribute(characterSheet.HP);
            SPBar.SetAttribute(characterSheet.SP);
        }
        
        //Switch on
        gameObject.SetActive(true);
        GUIManager.instance.menusVisible = true;
    }
    public void deactivate()
    {
        //Switch off
        gameObject.SetActive(false);
        GUIManager.instance.menusVisible = false;
    }
}