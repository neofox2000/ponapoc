using UnityEngine;
using SardonicMe.Perlib;
using System.Collections.Generic;
using RPGData;

[CreateAssetMenu(menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    #region Structures & Constants
    static class SaveKeys
    {
        public const string sk_Version = "saveVersion";
        public const string sk_DialogueData = "dialogueSaveData";

        public const string sk_Items = "items";
        public const string sk_ItemRecipes = "IREs";
        public const string sk_Quests = "quests";
        public const string sk_Quickslots = "quickslots";

        public const string sk_Stats = "stats";
        public const string sk_Skills = "skills";
        public const string sk_Abilities = "abilities";
        public const string sk_AvailableStatPoints = "availableStatPoints";
        public const string sk_AvailableSkillPoints = "availableSkillPoints";

        public const string sk_HP = "HP";
        public const string sk_SP = "SP";
        public const string sk_MP = "MP";
        public const string sk_CP = "CP";
        public const string sk_XP = "XP";

        public const string sk_Race = "race";
        public const string sk_Gender = "gender";
        
        public const string sk_PlayerLevel = "level";
        public const string sk_WorldPosition = "worldPosition";

        public const string sk_FogOfWarState = "FogOfWarState";
        public const string sk_LocationStates = "LocationStates";
        public const string sk_NPCData = "NPCData";
    }

    public const string baseSaveName = "ponisave";
    public const int currentSaveVersion = 6;
    const int defaultRaceID = 0;
    const string libKey = "awesomesauce";
    const string valueKey = "andthings";
    #endregion

    #region Properties
    //Change this into a singleton to allow stuff to run without MetagameManager
    //public static PlayerData instance = null;

    public CharacterSheet characterSheet;
    public Inventory inventory;
    public ItemRecipeProgressList IREManager;
    public Vector3 worldPosition;

    [HideInInspector]
    public AbilityConfig abilityConfig;
    public FogOfWar.FOWState[,] fogOfWarState;
    public List<LocationState> locationStates;
    public List<SaveQuest> quests;
    public QuickslotData quickslotData;
    public NPCData npcData;

    public WorldLocationEntrance placeToLoadInto { get; set; }
    #endregion

    #region Methods
    //Constructors
    public void Newgame(Vector3 startingWorldPosition)
    {
        //characterSheet = CreateInstance<CharacterSheet>();
        //inventory = CreateInstance<Inventory>();
        //if (IREManager == null) IREManager = new ItemRecipeProgressList();

        quests = new List<SaveQuest>();
        locationStates = new List<LocationState>();

        quickslotData = new QuickslotData();
        abilityConfig = new AbilityConfig();
        worldPosition = startingWorldPosition;
        fogOfWarState = new FogOfWar.FOWState[SaveFogOfWar.width, SaveFogOfWar.height];

        //Add first quest
        quests.Add(new SaveQuest(1, QuestState.Active, 0));

        //Setup location visibility
        WorldLocationTemplate[] locs = GameDatabase.sWorldLocations;
        for (int i = 0; i < locs.Length; i++)
        {
            //Iterate through each entrance
            for (int j = 0; j < locs[i].entrances.Length; j++)
            {
                //Only add the ones that are visible
                if (locs[i].entrances[j].newGameVisible)
                {
                    SetLocationVisible(
                        locs[i].saveId,
                        locs[i].entrances[j].saveId,
                        true);
                }
            }
        }
    }

    //Player Data Transfer Methods
    public void SaveFromController(Actor controller)
    {
        inventory.ClearAllEvents();
    }
    public void SaveFromMission(PlayerController PC)
    {
        characterSheet.RemoveAllStatusEffects(true);
        inventory.ClearAllEvents();
    }
    public void LoadToController(Actor controller)
    {
        controller.inventory = inventory;
    }

    //Location State Methods
    public LocationState GetCurrentLocationState()
    {
        if (placeToLoadInto != null)
            return GetLocationState(placeToLoadInto.saveId);
        else
            //Assume testing
            return GetLocationState(GameDatabase.metaGame.testEntrance.saveId);
    }
    public LocationState GetLocationState(int locationId)
    {
        //Load persistent storage from player data
        LocationState locationState = locationStates.
            Find(x => x.locationId == locationId);

        //Create mission data entry if one does not exist
        if (locationState == null)
        {
            //Create new data
            locationState = new LocationState(locationId);

            //Store new data
            locationStates.Add(locationState);
        }

        return locationState;
    }
    public bool GetLocationVisible(int locationId, int portalId)
    {
        LocationState ls = GetLocationState(locationId);
        return ls.portalAccesList.
            Find(x => x.portalId == portalId).enabled;
    }
    public void SetLocationVisible(int locationId, int portalId, bool visible)
    {
        GetLocationState(locationId).setPortalAccess(
            portalId, 
            visible);
    }

    //Quest State Methods
    void SetSaveQuests(SaveQuest[] saveQuests)
    {
        quests.Clear();
        if (saveQuests != null)
            quests.AddRange(saveQuests);
    }
    SaveQuest[] GetSaveQuests()
    {
        return quests.ToArray();
    }
    public void SetQuestState(QuestTemplate quest, QuestState newState, float timeStarted)
    {
        //Create new quest struct
        SaveQuest sq = new SaveQuest(quest.saveID, newState, timeStarted);

        //See if quest state is already present
        int index = quests.FindIndex(x => x.id == quest.saveID);

        //Update or add new quest State
        if (index >= 0) quests[index] = sq;
        else quests.Add(sq);
    }
    public QuestState GetQuestState(QuestTemplate quest)
    {
        if (quests != null)
        {
            SaveQuest sq = quests.Find(x => x.id == quest.saveID);
            if (sq.id != 0) return sq.state;
        }

        return QuestState.Inactive;
    }

    //Disk I/O
    public bool SaveToDisk(int slotNo)
    {
        if ((characterSheet != null) && (inventory != null) && (IREManager != null))
            return Save(MakeSaveFN(slotNo), MakeSnapSaveFN(slotNo));

        Debug.LogError("characterSheet, inventory or IREManager was null while trying to save!");
        return false;
    }
    public bool Save(string filename, string snapShotFileName)
    {
        try
        {
            #region Main Save File
            Perlib savefile = new Perlib(filename, libKey);
            if (!savefile.Exists) savefile.Open();
            else
            {
                //Erase and start over (in case of bad data)
                savefile.Delete();
                savefile = new Perlib(filename, libKey);
                savefile.Open();
            }

            #region Critical Save Data
            //Save game meta data
            savefile.SetValue(SaveKeys.sk_Version, currentSaveVersion);
            savefile.SetValue(SaveKeys.sk_Quests, GetSaveQuests());

            //Inventory
            savefile.SetValue(SaveKeys.sk_Items, inventory.GetSaveItems());

            //Character sheet data
            savefile.SetValue(SaveKeys.sk_Stats, characterSheet.GetSaveAttributes());
            savefile.SetValue(SaveKeys.sk_Skills, characterSheet.GetSaveSkills());
            savefile.SetValue(SaveKeys.sk_Abilities, characterSheet.GetSaveAbilities());

            savefile.SetValue(SaveKeys.sk_HP, new SaveAttribute(characterSheet.HP));
            savefile.SetValue(SaveKeys.sk_MP, new SaveAttribute(characterSheet.MP));
            savefile.SetValue(SaveKeys.sk_SP, new SaveAttribute(characterSheet.SP));
            //AP is always reset to full upon entering a mission, thus it is not needed in the save file
            savefile.SetValue(SaveKeys.sk_CP, new SaveAttribute(characterSheet.CP));

            savefile.SetValue(SaveKeys.sk_PlayerLevel, characterSheet.level);
            savefile.SetValue(SaveKeys.sk_XP, characterSheet.XP);
            savefile.SetValue(SaveKeys.sk_AvailableStatPoints, characterSheet.availableAttributePoints.valueBase);
            savefile.SetValue(SaveKeys.sk_AvailableSkillPoints, characterSheet.availableSkillPoints.valueBase);
            //savefile.SetValue(SaveKeys.sk_Race, characterSheet.race.saveID);
            //savefile.SetValue(SaveKeys.sk_Gender, characterSheet.gender);

            //Crafting progress data
            savefile.SetValue(SaveKeys.sk_ItemRecipes, IREManager.getSaveIREs());

            //Position of player on the world map
            savefile.SetValue(SaveKeys.sk_WorldPosition, worldPosition);

            //World map Fog of War data
            savefile.SetValue<SaveFogOfWar>(SaveKeys.sk_FogOfWarState, new SaveFogOfWar(fogOfWarState));

            //Mission data
            if (locationStates != null)    //New var
            {
                LocationStateDataPacket smdp = new LocationStateDataPacket(locationStates);
                savefile.SetValue<LocationStateDataPacket>(SaveKeys.sk_LocationStates, smdp); 
            }

            //NPC data
            savefile.SetValue<SaveNPCData>(SaveKeys.sk_NPCData, npcData.Pack());
            #endregion

            #region Non-Critical Save Data
            //Quickslot data
            savefile.SetValue(SaveKeys.sk_Quickslots, quickslotData.SerializeMe());
            #endregion

            savefile.Save();
            #endregion

            #region Snap Shot Data
            //Savegame Meta info for quickly displaying saves in the menu without actually having to open the entire save file (incurrs large overheads)
            Perlib snapfile = new Perlib(snapShotFileName);
            if (!snapfile.Exists) snapfile.Open();
            else
            {
                //Erase and start over (in case of bad data)
                snapfile.Delete();
                snapfile = new Perlib(snapShotFileName);
                snapfile.Open();
            }

            snapfile.SetValue(SaveKeys.sk_Version, currentSaveVersion);
            snapfile.SetValue(SaveKeys.sk_PlayerLevel, characterSheet.level);
            snapfile.SetValue(SaveKeys.sk_XP, characterSheet.XP);
            snapfile.Save();
            #endregion

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PlayerData.save()] --- " + e.Message);
            return false;
        }
    }
    public bool LoadFromDisk(int slotNo)
    {
        if (SaveExists(slotNo))
        {
            if (Load(MakeSaveFN(slotNo)))
            {
                characterSheet.OnDeserialized();
                IREManager.onDeserialized();

                //Add missing data
                if (abilityConfig == null) abilityConfig = new AbilityConfig();

                return true;
            }
        }

        return false;
    }
    public bool Load(string filename)
    {
        try
        {
            //Perlib savefile = new Perlib(filename, null, libKey, valueKey);
            Perlib savefile = new Perlib(filename, libKey);
            savefile.Open();

            #region Critical Save Data
            //bool versionMatch = savefile.GetValue<int>("saveVersion", 0) == currentSaveVersion;
            SetSaveQuests(savefile.GetValue<SaveQuest[]>(SaveKeys.sk_Quests, null));

            inventory.SetSaveItems(savefile.GetValue<SaveItem[]>(SaveKeys.sk_Items, null));
            characterSheet.SetSaveAttributes(savefile.GetValue<SaveAttribute[]>(SaveKeys.sk_Stats, null));
            characterSheet.SetSaveSkills(savefile.GetValue<SaveSkill[]>(SaveKeys.sk_Skills, null));
            characterSheet.SetSaveAbilities(savefile.GetValue<SaveAbility[]>(SaveKeys.sk_Abilities, null));

            savefile.GetValue<SaveAttribute>(SaveKeys.sk_HP, new SaveAttribute(characterSheet.HP)).Unpack(characterSheet.HP);
            savefile.GetValue<SaveAttribute>(SaveKeys.sk_MP, new SaveAttribute(characterSheet.MP)).Unpack(characterSheet.MP);
            savefile.GetValue<SaveAttribute>(SaveKeys.sk_SP, new SaveAttribute(characterSheet.SP)).Unpack(characterSheet.SP);
            //AP is always reset to full upon entering a mission, thus it is not needed in the save file
            savefile.GetValue<SaveAttribute>(SaveKeys.sk_CP, new SaveAttribute(characterSheet.CP)).Unpack(characterSheet.CP);

            characterSheet.level = savefile.GetValue<int>(SaveKeys.sk_PlayerLevel, characterSheet.level);
            characterSheet.XP = savefile.GetValue<float>(SaveKeys.sk_XP, characterSheet.XP);
            characterSheet.availableAttributePoints.valueBase = savefile.GetValue<float>(SaveKeys.sk_AvailableStatPoints, characterSheet.availableAttributePoints.valueBase);
            characterSheet.availableSkillPoints.valueBase = savefile.GetValue<float>(SaveKeys.sk_AvailableSkillPoints, characterSheet.availableSkillPoints.valueBase);

            if(savefile.HasKey(SaveKeys.sk_WorldPosition))
                worldPosition = savefile.GetValue<Vector3>(SaveKeys.sk_WorldPosition);

            fogOfWarState = savefile.GetValue<SaveFogOfWar>(SaveKeys.sk_FogOfWarState, new SaveFogOfWar()).unpack();
            IREManager.setSaveIREs(savefile.GetValue<SaveIRE[]>(SaveKeys.sk_ItemRecipes, null));

            //New var (Saved Mission Data)
            LocationStateDataPacket smdp = savefile.GetValue<LocationStateDataPacket>(SaveKeys.sk_LocationStates, new LocationStateDataPacket());
            locationStates = smdp.unpack();

            //NPC data
            npcData.Unpack(savefile.GetValue<SaveNPCData>(SaveKeys.sk_NPCData, new SaveNPCData()));
            #endregion

            #region Non-critical data
            //Quickslots
            quickslotData = new QuickslotData(savefile.GetValue<string>(SaveKeys.sk_Quickslots, string.Empty));
            #endregion

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PlayerData.load()] --- " + e.Message);
            return false;
        }
    }

    //Static Methods
    public static string MakeSaveFN(string extra, string extension)
    {
        return string.Concat(
            //Application.persistentDataPath + "/" + 
            baseSaveName, extra, extension);
    }
    public static string MakeSaveFN(int slotNo)
    {
        return MakeSaveFN((slotNo + 1).ToString("000"), ".sav");
    }
    public static string MakeSnapSaveFN(int slotNo)
    {
        return MakeSaveFN((slotNo + 1).ToString("000"), ".snp");
    }
    public static bool SaveExists(int slotNo)
    {
        //return Common.saveExists(makeSaveFN(slotNo));
        //return Perlib.Exists(makeSaveFN(slotNo));
        //return System.IO.File.Exists(makeSaveFN(slotNo));
        return new Perlib(MakeSaveFN(slotNo), libKey).Exists;
    }
    #endregion
}