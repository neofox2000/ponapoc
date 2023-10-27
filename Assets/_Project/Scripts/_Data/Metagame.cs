using UnityEngine;
using System.Collections.Generic;
using System;
using Variables;

[CreateAssetMenu(menuName = "Data/Metagame")]
public class Metagame : ScriptableObject
{
    #region Structures
    [System.Serializable]
    public enum MetaGameStates { Idle, TitleMenu, Abandon, NewGame, Map, Mission };
    #endregion

    #region Inspector Properties
    [Header("New Game Settings")]
    public StoryTemplate introStory;
    public Vector3 startingWorldPosition;
    public WorldLocationEntrance
        startingWorldLocationTutorial,
        startingWorldLocationNormal;

    [Header("Testing")]
    public WorldLocationEntrance testEntrance;
    public CharacterSheet testCharacterSheet;

    [Header("Fallbacks")]
    [Tooltip("Any code that uses a loot table can use this as a fallback if the dev forgot to assign it")]
    public LootTable fallbackLootTable;

    #endregion
    #region Private Properties

    bool _paused = false;
    public bool paused
    {
        get { return _paused; }
        set
        {
            if (value)
            {
                Time.timeScale = 0;
                AudioManager.instance.Pause();
            }
            else
            {
                Time.timeScale = 1;
                AudioManager.instance.Unpause();
            }

            _paused = value;
        }
    }

    //Stores NPC controllers to save when leaving an area
    List<NPCController> npcTrackingList = new List<NPCController>();

    //Easy way to determine what state the game is in
    MetaGameStates _currentState = MetaGameStates.TitleMenu;

    //Accessors
    public MetaGameStates currentState
    {
        get { return _currentState; }
        set
        {
            MetaGameStates oldState = _currentState;
            _currentState = value;

            if (OnCurrentStateChanged != null)
                OnCurrentStateChanged(oldState, _currentState);
        }
    }
    #endregion
    #region Events
    public delegate void CurrentStateChanged(MetaGameStates oldState, MetaGameStates newState);
    public event CurrentStateChanged OnCurrentStateChanged;
    public Action OnGameOptionsSaved;
    public Action OnGameOptionsLoaded;
    #endregion

    public void TrackNPC(NPCController npc)
    {
        //Add to GameManager's tracking list
        if (!npcTrackingList.Contains(npc))
            npcTrackingList.Add(npc);
    }
    public void ExitToWorldMap()
    {
        LoadingManager.SwitchScene(LoadingManager.SceneSelection.worldMap);
    }
    public void ExitMission(WorldLocationEntrance nextLocation, MetaGameStates exitState)
    {
        //Update state
        currentState = exitState;

        if ((exitState == MetaGameStates.TitleMenu) || (exitState == MetaGameStates.NewGame))
            LoadingManager.SwitchScene(LoadingManager.SceneSelection.title);
        else
        {
            //If the player died, restore them before transition
            ((PlayerController)GameDatabase.localPlayer)
                .PrepareForReturnToMap();

            //Save NPC Data
            foreach (NPCController npc in npcTrackingList)
                GameDatabase.sPlayerData.npcData.Store(npc);

            //Clear list (don't keep saving old data)
            npcTrackingList.Clear();

            //Determine what happens next
            if (exitState == MetaGameStates.Mission)
                //Start the next mission in the chain
                StartMission(nextLocation);
            else
                //Go back to world map
                ExitToWorldMap();
        }
    }
    public void StartMission(WorldLocationEntrance entryPoint)
    {
        GameDatabase.sPlayerData.placeToLoadInto = entryPoint;

        //Load mission scene
        LoadingManager.SwitchScene(LoadingManager.SceneSelection.mission);
    }
}