using UnityEngine;
using UnityEngine.EventSystems;

public class GUIManager : MonoBehaviour
{
    //Constants
    const float inputDelay = 0.2f;

    //Enums
    public enum GUISelection { Loading, Splash, Title, Story, WorldMap, Mission, Quitting, CharacterCreation };

    //Static Properties
    public static GUIManager instance = null;

    //Inspector Properties
    public Metagame metagame;
    public GUI_Loading guiLoading;
    public GUI_Splashscreen guiSplash;
    public GUI_TitleScreen guiTitle;
    public GUI_StoryManager guiStory;
    public GUI_WorldMap guiWorldMap;
    public HUD_Connector guiMission;
    public GUI_QuittingScreen guiQuitting;
    public GUI_Common guiCommon;

    //Private Properties
    BaseInputModule inputModule;
    ReSelector reSelector = null;
    bool inConversation = false;

    //Accessors
    float _menuDelayTimer = 0;
    float menuDelayTimer
    {
        get { return _menuDelayTimer; }
        set
        {
            _menuDelayTimer = value;
            if (inputModule)
                inputModule.enabled = value <= 0;
        }
    }
    bool _menusVisible = false;
    public bool menusVisible
    {
        get { return _menusVisible; }
        set
        {
            _menusVisible = value;

            //Don't let this run if there are no UI elements to apply it to
            if (reSelector)
                reSelector.enabled = value;
        }
    }
    public bool allowPlayerInput
    {
        get
        {
            return
                !guiCommon.IsAnythingShowing() &&
                !menusVisible &&
                !inConversation &&
                (menuDelayTimer <= 0);
        }
    }
    public bool allowMenuInput
    {
        get
        {
            return
                !inConversation &&
                (menusVisible || guiCommon.IsAnythingShowing()) &&
                (menuDelayTimer <= 0);
        }
    }

    //Methods
    void toggleLoadingGUI(bool show)
    {
        if (show)
        {
            guiCommon.setBackground(true, Color.black);
            guiLoading.activate();
        }
        else
            guiLoading.deactivate();
    }
    void toggleStoryGUI(bool show)
    {
        if (show)
            guiCommon.setBackground(true, Color.black);
    }
    void toggleSplashGUI(bool show)
    {
        if (show)
            guiSplash.activate();
        else
            guiSplash.deactivate();
    }
    void toggleTitleGUI(bool show)
    {
        if (show)
            guiTitle.activate();
        else
            guiTitle.deactivate();
    }
    void toggleWorldMapGUI(bool show)
    {
        if (show)
            guiWorldMap.activate();
        else
            guiWorldMap.deactivate();
    }
    void toggleMissionGUI(bool show)
    {
        if (show)
            guiMission.Activate();
        else
            guiMission.Deactivate();
    }
    void toggleQuittingGUI(bool show)
    {
        if (show)
        {
            guiCommon.setBackground(true, Color.black);
            guiQuitting.gameObject.SetActive(true);
        }
        else
            guiQuitting.gameObject.SetActive(false);
    }
    void toggleCharacterCreationGUI(bool show)
    {
        guiCommon.setBackground(false, Color.black);
        guiCommon.OnCharacterCreation(show);
    }
    public void toggleGUI(GUISelection gui, bool show)
    {
        switch (gui)
        {
            case GUISelection.Loading: toggleLoadingGUI(show); break;
            case GUISelection.Splash: toggleSplashGUI(show); break;
            case GUISelection.Title: toggleTitleGUI(show); break;
            case GUISelection.Story: toggleStoryGUI(show); break;
            case GUISelection.WorldMap: toggleWorldMapGUI(show); break;
            case GUISelection.Mission: toggleMissionGUI(show); break;
            case GUISelection.Quitting: toggleQuittingGUI(show); break;
            case GUISelection.CharacterCreation: toggleCharacterCreationGUI(show); break;
        }
    }

    public void PlayStory(StoryTemplate story)
    {
        guiStory.PlayStory(story);
    }

    public void UpdatePauseState()
    {
        metagame.paused =
            (LoadingManager.currentScene != LoadingManager.SceneSelection.charCreation) &&
            (!guiCommon.IsBartering()) &&
            (
                (guiCommon.IsAnythingShowing())
                || menusVisible 
                //|| inConversation
            );
    }
    public void OnConversationStart()
    {
        inConversation = true;
        //UpdatePauseState();
        CameraController.instance.enabled = false;
    }
    public void OnConversationEnd()
    {
        menuDelayTimer = inputDelay;
        inConversation = false;
        //UpdatePauseState();
        CameraController.instance.enabled = true;
    }
    public void OnShowCommonGUI()
    {
        menusVisible = true;
        UpdatePauseState();
    }
    public void OnHideCommonGUI()
    {
        menuDelayTimer = inputDelay;
        EventSystem.current.SetSelectedGameObject(null);
        menusVisible = false;
        UpdatePauseState();
    }

    bool WasHardMenuButtonUsed()
    {
        return
            //Keyboard
            InputX.Down(InputCode.Escape) ||

            //Controller
            InputX.Down(InputCode.XboxBackButton);

            //Mouse (Only for closing menus)
            //(guiCommon.IsAnythingShowing() && InputX.Up(InputCode.MouseClickRight));
    }
    void HandleHardMenuButton()
    {
        if (guiCommon.IsBartering())
            guiCommon.guiBarter.CancelTrade();
        else
            guiCommon.OnMenuButtonPressed();
    }
    bool WasSoftMenuButtonUsed()
    {
        return
            //Keyboard
            InputX.Down(InputCode.Tab) ||

            //Controller
            InputX.Down(InputCode.XboxStartButton);
    }
    void HandleSoftMenuButton()
    {
        //Don't do anything if not in a mission
        if (GameManager.instance != null)
        {
            if (!guiCommon.IsNonCharacterShowing())
                guiCommon.OnToggleCharacter();
            else
            {
                if (guiCommon.IsBartering())
                    guiCommon.guiBarter.CancelTrade();
                else
                {
                    guiCommon.OnCloseAll();
                    guiCommon.OnCharacter(true);
                }
            }
        }
    }
    bool WasHelpButtonUsed()
    {
        return
            //Keyboard
            InputX.Down(KeyCode.F1);
    }
    void HandleHelpButton()
    {
        guiCommon.OnToggleHelp();
    }

    void Awake()
    {
        if (!instance)
        {
            instance = this;
            reSelector = GetComponent<ReSelector>();
            inputModule = EventSystem.current.currentInputModule;
        }
        else
            Destroy(gameObject);
    }
    void Start()
    {
        guiCommon.OnShow.AddListener(OnShowCommonGUI);
        guiCommon.OnHide.AddListener(OnHideCommonGUI);
    }
    void Update()
    {
        if (menuDelayTimer > 0)
            menuDelayTimer -= Time.unscaledDeltaTime;

        //Only do stuff when on the world map or in a mission
            if(metagame.currentState != Metagame.MetaGameStates.Map &&
               metagame.currentState != Metagame.MetaGameStates.Mission)
            return;

        //Menu button handlers
        if ((!inConversation || guiCommon.IsBartering()) && (menuDelayTimer <= 0))
        {
            if (WasHardMenuButtonUsed())
                HandleHardMenuButton();

            if (WasSoftMenuButtonUsed())
                HandleSoftMenuButton();

            if (WasHelpButtonUsed())
                HandleHelpButton();
        }
    }
}