using UnityEngine;
using System.Collections.Generic;
using RPGData;
using Variables;
using Events;

[System.Serializable]
public class GameDatabase : ScriptableObject
{   
    //In Editor Paths
    public static string dbFile_editor = @"Assets/Resources/GameDatabase.asset";
    static string dbFile_runtime = "GameDatabase";

    //Constants that can't be pre-declared
    public static int playerLayer;

    //Static access
    static GameDatabase _core = null;

    public static GameDatabase core
    {
        get
        {
            if (!_core) _core = Resources.Load<GameDatabase>(dbFile_runtime);
            return _core;
        }
    }

    //Quick Accessors
    public static GameSettings sGameSettings { get { return core.gameSettings; } }
    public static PlayerData sPlayerData { get { return core.playerData; } }
    public static Inventory lInventory { get { return core.playerData.inventory; } }
    public static CharacterSheet lCharacterSheet { get { return core.playerData.characterSheet; } }
    public static AttributeTemplate[] sAttributes { get { return core.attributes; } }
    public static AbilityTemplate[] sAbilities { get { return core.abilities; } }
    public static ItemTemplate[] sItems { get { return core.items; } }
    public static QuestTemplate[] sQuests { get { return core.quests; } }
    public static AttributeTemplate[] sSkills { get { return core.skills; } }
    public static WorldLocationTemplate[] sWorldLocations { get { return core.worldLocations; } }
    public static Metagame metaGame { get { return core.metaGameSettings; } }

    public static PlayerController _localPlayer = null;
    public static PlayerController localPlayer
    {
        get { return _localPlayer; }
        set
        {
            if (value == null) core.beforePlayerDespawned.Raise();

            _localPlayer = value;

            if (value != null) core.afterPlayerSpawned.Raise();
        }
    }

    [Header("Main Data")]
    public GameObject playerPrefab;
    public PlayerData playerData;
    public GameSettings gameSettings;
    public Metagame metaGameSettings;

    [Header("Tables")]
    public AttributeTemplate[] attributes;
    public AbilityTemplate[] abilities;
    public ItemTemplate[] items;
    public QuestTemplate[] quests;
    public AttributeTemplate[] skills;
    public WorldLocationTemplate[] worldLocations;

    [Header("Events")]
    [Tooltip("Called after the player has been spawned")]
    public GameEvent afterPlayerSpawned;
    [Tooltip("Called before the player is de-spawned")]
    public GameEvent beforePlayerDespawned;

    [Header("Other")]
    public GameObject lootBagPrefab;
    [Tooltip("The layers that count as a valid location/room (used for detection on BaseController")]
    public LayerMask locationLayer;
    [Tooltip("Icon to use when none is available")]
    public Sprite defaultIcon;
    [Tooltip("Special effects that can get spawned by anything at any time")]
    public List<GameObject> effects;

    [Header("Testing")]
    public GameObject gameManagerPrefab;
    public GameObject missionCamPrefab;

    //Things that could not be called from the constructor
    public void OnEnable()
    {
        playerLayer = LayerMask.NameToLayer("Player");

#if UNITY_EDITOR
        //Warn developers of duplicate skill saveid cases
        if(skills != null)
        for (int i = 0; i < skills.Length; i++)
            for (int j = 0; j < skills.Length; j++)
                    if ((skills[i] != null) && (skills[j] != null))
                        if ((skills[i] != skills[j]) && (skills[i].saveID == skills[j].saveID))
                            Debug.LogWarning("Skill " + skills[i].name + " and " + skills[j].name + " share the same SaveID!");

        //Warn developers of duplicate quest saveid cases
        if(quests != null)
        for (int i = 0; i < quests.Length; i++)
            for (int j = 0; j < quests.Length; j++)
                    if ((quests[i] != null) && (quests[j] != null))
                        if ((quests[i] != quests[j]) && (quests[i].saveID == quests[j].saveID))
                            Debug.LogWarning("Quest '" + quests[i].name + "' and '" + quests[j].name + "' share the same SaveID!");
#endif
    }

    public void NewGame(bool doTutorial, bool testing = false)
    {
        //Set player data to a newgame state
        sPlayerData.Newgame(metaGame.startingWorldPosition);

        LoadingManager.SceneSelection levelToLoad = LoadingManager.SceneSelection.mission;
        if (doTutorial)
        {
            sPlayerData.placeToLoadInto = metaGame.startingWorldLocationTutorial;
            GUIManager.instance.PlayStory(metaGame.introStory);
        }
        else
        {
            sPlayerData.placeToLoadInto = metaGame.startingWorldLocationNormal;

            //Prevent double loadlevel in testing scenarios
            if (!testing)
                LoadingManager.SwitchScene(levelToLoad);
        }
    }

    //Persistence Methods
    public bool LoadGame(int slotNo)
    {
        return sPlayerData.LoadFromDisk(slotNo);
    }
    public bool SaveGame(int slotNo)
    {
        return sPlayerData.SaveToDisk(slotNo);
    }
    public void SetConfigKey(string key, float value)
    {
        gameSettings.SetKey(key, value);
    }
}