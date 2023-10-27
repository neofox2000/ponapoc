using UnityEngine;
using System.Collections;
using Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cheat Code Stuff")]
    public InventoryManifest ammoCheatItems;
    public InventoryManifest moneyCheatItems;

    //PlayerController localPlayer = null;
    bool playerSpawnedInMission = false;
    WorldLocationEntrance nextLocation;
    Metagame.MetaGameStates missionExitState;


    //public PlayerController GetLocalPlayer() { return localPlayer; }
    void ExitMission()
    {
        AudioManager.instance.StopAll();
        GameDatabase.metaGame.ExitMission(
            nextLocation, 
            missionExitState);
    }

    /// <summary>
    /// Exiting location via menu or other mechanism.
    /// Always returns to world map.
    /// </summary>
    /// <param name="exitState"></param>
    public void ExitMission(Metagame.MetaGameStates exitState)
    {
        playerSpawnedInMission = false;
        missionExitState = exitState;
        ExitMission();
    }
    /// <summary>
    /// Exiting location via portal.
    /// Can go to world map or another location depending on portal setting
    /// </summary>
    /// <param name="portal"></param>
    public void ExitMission(LocationPortal portal)
    {
        if (portal.portalType == LocationPortal.PortalTypes.Map)
        {
            //Go to world map
            ExitMission(Metagame.MetaGameStates.Map);
        }
        else
        {
            //Go to another location
            missionExitState = Metagame.MetaGameStates.Mission;
            nextLocation = portal.portalTarget;

            //missionToLoad = DB_Missions.instance.lookup(nextMission);
            ExitMission();
        }
    }
    public Location CreateLocation(GameObject prefab)
    {
        //Create new location parented to me
        if (prefab == null)
        {
            Debug.LogWarning("Location Prefab is not set!");
            return null;
        }

        //Return reference to newly created location
        return ((GameObject)Instantiate(prefab, transform)).GetComponent<Location>();
    }
    IEnumerator InitMission()
    {
        //Wait 1 frame for all scripts to awake
        yield return new WaitForEndOfFrame();

        //Player Spawning
        if (!playerSpawnedInMission)
        {
            //Determine which Portal to spawn on
            LocationPortal lp;
            if (LoadingManager.awoken)
            {
                //Normal Init
                lp = LocationPortal.Find(GameDatabase.sPlayerData.placeToLoadInto);
            }
            else
            {
                //Testing Input Init
                GetComponent<InputX>().enabled = true;

                //Testing load game settings
                GameDatabase.sGameSettings.Load();

                //Use the testing character sheet
                GameDatabase.sPlayerData.characterSheet.CopyFrom(GameDatabase.metaGame.testCharacterSheet, false);

                lp = LocationPortal.Find(GameDatabase.metaGame.testEntrance);
                if(lp == null)
                {
                    //Testing Init
                    Debug.Log("Spawning player at first available portal...");
                    lp = LocationPortal.Find(null);
                }
            }

            //Set default spawn position and direction
            Vector3 spawnPosition = Vector3.zero;
            Direction spawnDirection = Direction.Right;

            //Get non-default position/direction from portal (if any)
            if (lp != null)
            {
                //Position
                spawnPosition = lp.transform.position;

                //Direction
                if (lp.directionToFace != Direction.None)
                    spawnDirection = lp.directionToFace;
            }

            //Spawn parented at position            
            SpawnPlayer(spawnPosition, spawnDirection);

            //Update game state
            GameDatabase.metaGame.currentState = Metagame.MetaGameStates.Mission;
        }
    }
    void SpawnPlayer(Vector3 spawnPosition, Direction spawnDirection)
    {
        //Spawn parented at position            
        GameObject playerGO = Common.ProducePrefab(
            //Prefab
            GameDatabase.core.playerPrefab,

            //Position
            spawnPosition,

            //Rotation
            Quaternion.identity,

            //Use Pool
            false,

            //Parent
            transform);

        //Set facing direction
        playerGO.GetComponent<Actor>().dir = (int)spawnDirection;

        //Set flag
        playerSpawnedInMission = true;
    }

    void Start() 
    {
        if (!instance)
        {
            instance = this;

            StartCoroutine(InitMission());
        }
        else
            Destroy(gameObject);
	}
    void OnDisable()
    {
        GameDatabase.localPlayer = null;
    }
}