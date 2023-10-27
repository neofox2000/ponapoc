using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class LoadingManager : MonoBehaviour
{
    //Enums
    public enum SceneSelection { loading, splash, title, storyTeller, worldMap, mission, quitting, charCreation };
    public enum InputMethod { KeyMouse, Controller }

    //Constants
    const float
        cursorHideDelay = 2.5f;
    const string
        sceneLoader = "Loader",
        sceneManagers = "Managers",
        sceneGUI = "GUI",
        sceneWorldMap = "WorldView",
        sceneCharacterCreation = "Character_Creation";

    //Static Properties
    public static bool awoken = false;
    private static bool wakeUp = false;
    public static SceneSelection currentScene = SceneSelection.loading;
    private static SceneSelection sceneToLoad = SceneSelection.splash;
    public static InputMethod currentInputMethod = InputMethod.KeyMouse;

    //Inspector Properties
    public bool missionTesting = false;
    public bool skipSplash = false;

    //Private Properties
    bool doStartup = false;
    float cursorHideTimer = 0;
    Vector3 lastMousePosition;
    List<string> scenesToUnload = new List<string>();
    List<GUIManager.GUISelection> guisToDisable = new List<GUIManager.GUISelection>();

    //Private Methods
    void DisableGUIs()
    {
        foreach (GUIManager.GUISelection g in guisToDisable)
            GUIManager.instance.toggleGUI(g, false);

        guisToDisable.Clear();
        GUIManager.instance.guiCommon.OnMainMenu(false);

        //Clear any fade effect that was used to transition out of previous scenes
        GUIManager.instance.guiCommon.OnFadeOut(false, 1);
    }
    AsyncOperation[] UnloadScenes()
    {
        AsyncOperation[] blah = new AsyncOperation[scenesToUnload.Count];
        for (int i = 0; i < scenesToUnload.Count; i++)
            blah[i] = SceneManager.UnloadSceneAsync(scenesToUnload[i]);

        scenesToUnload.Clear();
        return blah;
    }
    /// <summary>
    /// Make sure no unwanted scenes are present (usually left in while playtesting in editor)
    /// </summary>
    void UnloadEditorScenes()
    {
        //Check for a
        if (SceneManager.GetSceneByName(sceneWorldMap).isLoaded)
        {
            Debug.LogWarning("Left sceneWorldMap loaded in editor");
            //SceneManager.UnloadScene(sceneWorldMap);
            SceneManager.UnloadSceneAsync(sceneWorldMap);
        }
    }
    IEnumerator LoadScenesCoroutine()
    {
        DisableGUIs();

        yield return new WaitForEndOfFrame();

        AsyncOperation[] unloadOperations = null;
        if (scenesToUnload.Count > 0)
            unloadOperations = UnloadScenes();

        //Allow unload to work it's magic before continuing
        if (unloadOperations == null)
            yield return new WaitForEndOfFrame();
        else
        {
            //Setup variables that will be used
            int i;
            bool doneLoading = false;

            //Start checking loop
            while (!doneLoading)
            {
                //Wait 1 frame
                yield return new WaitForEndOfFrame();

                //Check for loading completion
                for (i = 0; i < unloadOperations.Length; i++)
                {
                    //Break if any operations are still unfinished
                    if (!unloadOperations[i].isDone)
                        break;

                    //All operations have finished (or we would have broken the loop)
                    doneLoading = true;
                }
            }
        }

        //Unload assets and wait for finish
        AsyncOperation unloadAssetsOp = Resources.UnloadUnusedAssets();
        while (!unloadAssetsOp.isDone)
            yield return new WaitForEndOfFrame();

        //Give the GUIs some time to do cleanup animations and so on
        yield return new WaitForSecondsRealtime(0.5f);

        switch (sceneToLoad)
        {
            case SceneSelection.splash:
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.Splash, true);
                guisToDisable.Add(GUIManager.GUISelection.Splash);
                break;

            case SceneSelection.title:
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.Title, true);
                guisToDisable.Add(GUIManager.GUISelection.Title);
                break;

            case SceneSelection.storyTeller:
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.Story, true);
                guisToDisable.Add(GUIManager.GUISelection.Story);
                break;

            case SceneSelection.worldMap:
                scenesToUnload.Add(sceneWorldMap);
                SceneManager.LoadScene(sceneWorldMap, LoadSceneMode.Additive);
                //sceneToSetActive = SceneManager.GetSceneByName(sceneWorldMap);
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.WorldMap, true);
                guisToDisable.Add(GUIManager.GUISelection.WorldMap);
                break;

            case SceneSelection.mission:
                string missionScene = GameDatabase.sPlayerData.placeToLoadInto.parentLocation.sceneName;
                scenesToUnload.Add(missionScene);
                SceneManager.LoadScene(missionScene, LoadSceneMode.Additive);
                //sceneToSetActive = SceneManager.GetSceneByName(missionScene);
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.Mission, true);
                guisToDisable.Add(GUIManager.GUISelection.Mission);
                break;

            case SceneSelection.quitting:
                GUIManager.instance.toggleGUI(GUIManager.GUISelection.Quitting, true);
                guisToDisable.Add(GUIManager.GUISelection.Quitting);
                break;
            case SceneSelection.charCreation:
                scenesToUnload.Add(sceneCharacterCreation);
                SceneManager.LoadScene(sceneCharacterCreation, LoadSceneMode.Additive);
                //sceneToSetActive = SceneManager.GetSceneByName(sceneCharacterCreation);
                guisToDisable.Add(GUIManager.GUISelection.CharacterCreation);
                break;
        }

        currentScene = sceneToLoad;
        yield return new WaitForEndOfFrame();

        GUIManager.instance.toggleGUI(GUIManager.GUISelection.Loading, false);
    }
    IEnumerator WaitForManagersToLoad()
    {
        while (!AudioManager.instance)
            yield return new WaitForEndOfFrame();

        doStartup = true;

#if UNITY_EDITOR
        //Normal test startup
        if (missionTesting)
        {
            //Init persistent data in testing mode
            //GameDatabase.metaGame.setupForTesting();
            sceneToLoad = SceneSelection.mission;
        }
        else
        {
            if (skipSplash)
                sceneToLoad = SceneSelection.title;
        }
#endif
    }

    //Static Methods
    public static void SwitchScene(SceneSelection scene)
    {
        sceneToLoad = scene;
        wakeUp = true;
    }
    /// <summary>
    /// Make sure pre-requisite scenes are loaded
    /// </summary>
    public static void LoadManagerScenes()
    {
        if (!SceneManager.GetSceneByName(sceneManagers).isLoaded)
            SceneManager.LoadScene(sceneManagers, LoadSceneMode.Additive);
        if (!SceneManager.GetSceneByName(sceneGUI).isLoaded)
            SceneManager.LoadScene(sceneGUI, LoadSceneMode.Additive);
    }

    //Private Methods
    void ResetCursorHideTimer()
    {
        cursorHideTimer = cursorHideDelay;
    }
    void HideCursor()
    {
        Cursor.visible = false;
    }
    void ShowCursor()
    {
        Cursor.visible = true;
    }

    //SceneManager Events
    void ActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        //Debug.Log("Active Scene changed from " + oldScene.name + " to " + newScene.name);
    }
    void SceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        SceneManager.SetActiveScene(scene);
    }
    void SceneUnloaded(Scene scene)
    {
        //Debug.Log("Scene unloaded: `" + scene.name + "`");
    }

    //Mono Methods
    void OnEnable()
    {
        //Subscribe to SceneManager events
        SceneManager.activeSceneChanged += ActiveSceneChanged;
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }
    void OnDisable()
    {
        //Unsubscribe from SceneManager events
        SceneManager.activeSceneChanged -= ActiveSceneChanged;
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneUnloaded -= SceneUnloaded;
    }
    void Awake()
    {
#if !UNITY_EDITOR
        //Keep cursor within the game window (even in fullscreen mode?)
        Cursor.lockState = CursorLockMode.Confined;
#endif

        //Show cursor at startup
        ShowCursor();

        //Cache the mouse position
        lastMousePosition = Input.mousePosition;

        //Reset cursor hide timer
        ResetCursorHideTimer();

        //Setup input
        InputManager.LoadConfig();

        //Marks a normal game startup
        awoken = true;
    }
    void Start()
    {
        UnloadEditorScenes();
        LoadManagerScenes();

        StartCoroutine(WaitForManagersToLoad());
    }
    void Update()
    {
        //Wait for GUIManager to initialize (Only needed once)
        if (doStartup)
        {
            if(GUIManager.instance != null)
            {
                doStartup = false;
                wakeUp = true;
            }
        }

        //Listen for loading requests
        if (wakeUp)
        {
            wakeUp = false;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneLoader));
            GUIManager.instance.toggleGUI(GUIManager.GUISelection.Loading, true);
            AudioManager.instance.StopAll();

            StartCoroutine(LoadScenesCoroutine());
        }

        //Was the mouse clicked or moved?
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.mousePosition != lastMousePosition)
        {
            //Show Cursor
            ShowCursor();
            //Reset Timer
            ResetCursorHideTimer();
            //Cache position
            lastMousePosition = Input.mousePosition;
            //Cache input method
            currentInputMethod = InputMethod.KeyMouse;
        }

        //Was a controller button pressed?
        if (
            InputX.Down(InputCode.XboxBackButton) ||
            InputX.Down(InputCode.XboxButtonA) ||
            InputX.Down(InputCode.XboxButtonB) ||
            InputX.Down(InputCode.XboxButtonX) ||
            InputX.Down(InputCode.XboxButtonY) ||
            InputX.Down(InputCode.XboxInvertedRightStickY) ||
            InputX.Down(InputCode.XboxLeftBumper) ||
            InputX.Down(InputCode.XboxLeftStickClick) ||
            InputX.Down(InputCode.XboxLeftStickX) ||
            InputX.Down(InputCode.XboxLeftStickY) ||
            InputX.Down(InputCode.XboxLeftTrigger) ||
            InputX.Down(InputCode.XboxRightBumper) ||
            InputX.Down(InputCode.XboxRightStickClick) ||
            InputX.Down(InputCode.XboxRightStickX) ||
            InputX.Down(InputCode.XboxRightStickY) ||
            InputX.Down(InputCode.XboxRightTrigger) ||
            InputX.Down(InputCode.XboxStartButton) ||
            (Mathf.Abs(InputX.Axis(AxisCode.Joystick6)) > 0.2f) ||
            (Mathf.Abs(InputX.Axis(AxisCode.Joystick7)) > 0.2f)
        )
        {
            //Cache input method
            currentInputMethod = InputMethod.Controller;
        }

        //Handle cursor hide timer
        if (cursorHideTimer > 0)
        {
            cursorHideTimer -= Time.fixedDeltaTime;

            if (cursorHideTimer < 0)
            {
                //Hide cursor
                HideCursor();
            }
        }
    }
}