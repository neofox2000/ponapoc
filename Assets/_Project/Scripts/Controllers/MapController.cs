using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using GameDB;

public class MapController : MonoBehaviour 
{
    #region Structures
    [System.Serializable]
    public class EncounterSettings
    {
        public float chance = 0.0025f;
        public float cooldown = 6f;
        public AudioGroupTemplate alertSound;
        public int defaultEncounterMissionID = 12;
    }
    [System.Serializable]
    public class ScrollingSettings
    {
        public Vector2 mouseSpeed = new Vector2(2f, 2f);
        public float mouseSpeedLimit = 0.5f;
        public float mouseMoveCutoff = 5f;
        public int edgeScrollPixels = 16;
        public float scrollSpeed = 7.5f;
        public CommonStructures.Bounds2d contraints;
        public float zoomSpeed = 5f;
        public Vector2 zoomConstraints = new Vector2(5, 20);
    }
    [System.Serializable]
    public class TravelSettings
    {
        public float mapPathingHeightOffset = 2f;
        public float playerMoveSpeed = 2f;
        public float travelTimeMultiplier = 3f;
        public EncounterSettings encounterSettings;
    }
    [System.Serializable]
    public class MapLocationVisuals
    {
        public string name;
        public Texture2D mainArt;
        public Texture2D labelArt;
        public Vector3
            mainArtPosition,
            mainArtScale = new Vector3(1,1,1),
            labelArtPosition,
            labelArtScale = new Vector3(1,1,1);
    }
    #endregion

    public static MapController instance;

    public Camera mapCam;
    public PlayerMarker playerMarker;
    public Transform mapLocationsContainer;
    public GameObject
        mapLocationPrefab,
        fogOfWarPrefab;
    //public MapLocationVisuals[] mapLocationVisuals;
    public ScrollingSettings scrollingSettings;
    public TravelSettings travelSettings;

    [HideInInspector]
    public bool allowInput = true;

    bool allowMouseClick = true;
    float journeyStartTime, journeyLength;
    float randomEncounterTimer = 0;
    FogOfWar fogOfWar;
    Transform 
        playerMarkerTrans,
        camTrans;
    Vector3 mouseDownPosition;
    WorldLocation[] guiMapLocations;

    /*
    GUI_MapLocation createMapLocation()
    {
        GameObject GO = Instantiate<GameObject>(mapLocationPrefab);
        GO.transform.SetParent(mapLocationsContainer.transform);
        mapLocations.Add(GO.GetComponent<GUI_MapLocation>());
        return (mapLocations[mapLocations.Count - 1]);
    }

    public void setupMapLocations()
    {
        GUI_MapLocation mapLocation = null;
        //List<int> locationList = new List<int>();
        int i, c = 0;

        //Hide All Locations
        for (i = 0; i < mapLocations.Count; i++)
            mapLocations[i].hide();

        for (i = 0; i < MapLocations.Instance.Rows.Count; i++)
        {
            c++;
            if (mapLocations.Count < c)
                mapLocation = createMapLocation();
            else
                mapLocation = mapLocations[c - 1];

            //mapLocation.locationID = (int)i;
            mapLocation.locationID = i;
            mapLocation.show(true);
            //locationList.Add((Missions.rowIds)i);
        }

        //availableMissions = locationList;
    }
     */
    void Awake()
    {
        instance = this;
        guiMapLocations = mapLocationsContainer.GetComponentsInChildren<WorldLocation>();
        camTrans = mapCam.transform;

        fogOfWar = Instantiate<GameObject>(fogOfWarPrefab).GetComponent<FogOfWar>();
        fogOfWar.transform.SetParent(transform);

        playerMarkerTrans = playerMarker.transform;

        resetEncounterTimer();

        GUIManager.instance.guiWorldMap.OnMapAreaPointerEnter.AddListener(OnMapAreaPointerEnter);
        GUIManager.instance.guiWorldMap.OnMapAreaPointerExit.AddListener(OnMapAreaPointerExit);
        GUIManager.instance.guiWorldMap.OnCenterMap.AddListener(centerOnPlayer);
    }
    void Start()
    {
        OnPlayerDataChanged();
        EventSystem.current.SetSelectedGameObject(null);
    }
    void Update()
    {
        HandleInput();
        if (playerMarker.travelling)
        {
            doTravelStuff();
            updatePlayerData();
        }
    }
    void OnDestroy()
    {
        GUIManager.instance.guiWorldMap.OnMapAreaPointerEnter.RemoveListener(OnMapAreaPointerEnter);
        GUIManager.instance.guiWorldMap.OnMapAreaPointerExit.RemoveListener(OnMapAreaPointerExit);
        GUIManager.instance.guiWorldMap.OnCenterMap.RemoveListener(centerOnPlayer);

        instance = null;
    }
    
    public void showAlert(WorldMap_ProximityTrigger.AlertTriggerAction alertData)
    {
        Alerter.ShowMessage(alertData.text, alertData.duration);
    }
    public void startTravellingTo(Vector3 destination)
    {
        if (playerMarker.travelling)
        {
            stopTravelling();
        }

        playerMarker.setPlayerTravelTarget(destination);
        playerMarker.startTravelling();
    }
    public void stopTravelling(bool forceStop = false)
    {
        playerMarker.stopTravelling(forceStop);
        updatePlayerData();
    }
    void resetEncounterTimer()
    {
        //Add a delay between encounters
        randomEncounterTimer = travelSettings.encounterSettings.cooldown;
    }
    void doTravelStuff()
    {
        //Tick down player's sugar level
        GameDatabase.sPlayerData.characterSheet.Tick(
            Time.deltaTime * travelSettings.travelTimeMultiplier,
            true);

        //Generate random encounters    
        if (randomEncounterTimer > 0)
            randomEncounterTimer -= Time.deltaTime;
        else
        {
            if (Random.Range(0f, 1f) < travelSettings.encounterSettings.chance)
            {
                //Stop travelling
                stopTravelling(true);

                //Do a brief notification thingee for the player before launching the encounter mission itself
                StartCoroutine(startEncounter());

                resetEncounterTimer();
            }
        }
    }
    IEnumerator startEncounter()
    {
        allowInput = false;
        Alerter.ShowMessage("Suddenly, a wild pack of Trotters appears!");
        AudioManager.instance.Play(travelSettings.encounterSettings.alertSound);
        yield return new WaitForEndOfFrame();

        //int encounterToUse = travelSettings.encounterSettings.defaultEncounterMissionID;

        if (playerMarker.currentArea != null)
        {
            if (playerMarker.currentArea.encounters.Length > 0)
            {
                //encounterToUse = playerMarker.currentArea.encounters[
                    //Random.Range(0, playerMarker.currentArea.encounters.Length)];
            }
            else
                Debug.Log("Area has no encounters setup: " + playerMarker.currentArea.name);
        }

        //Disabled until mission -> world map location conversion is done
        //GUIManager.instance.guiWorldMap.OnEnterLocation(DB_Missions.instance.lookup(encounterToUse));
    }

    bool checkMouseMovedFarEnough()
    {
        return scrollingSettings.mouseMoveCutoff > (Vector3.Distance(mouseDownPosition, Input.mousePosition));
    }
    void HandleInput()
    {
        if ((allowInput) && (!GUI_Common.instance.IsAnythingShowing()))
        {
            int scrollModifier = 1;

            if (InputManager.dash.held())
                scrollModifier = 2;

            //Track mouse input specifically
            if (InputX.Down(InputCode.MouseClickLeft))
                mouseDownPosition = Input.mousePosition;

            //Quick-click -> Move player Marker
            if (InputX.Up(InputCode.MouseClickLeft) && checkMouseMovedFarEnough() && allowMouseClick)
                OnPositionClicked(Input.mousePosition);

            //Controller travel
            if (!InputX.Down(InputCode.MouseClickLeft) && InputManager.firePrimary.down())
                OnPositionClicked(new Vector3(Screen.width / 2, Screen.height / 2));

            //Map Centering
            if (InputManager.useConsumable.down())
                GUIManager.instance.guiWorldMap.centerMap();

            //Zooming
            zoom(InputX.Axis(AxisCode.MouseScroll));

            //Click-and-hold drag scrolling
            if (InputX.Pressed(InputCode.MouseClickLeft) && allowMouseClick)
            {
                scroll(
                    -InputX.Axis(AxisCode.MouseX) * scrollingSettings.mouseSpeed.x * Time.deltaTime,
                    InputX.Axis(AxisCode.MouseY) * scrollingSettings.mouseSpeed.y * Time.deltaTime);

                return;
            }

            //Keyboard / Controller scrolling
            float hor = InputManager.moveLeft.axisValue(); 
            float ver = InputManager.moveUp.axisValue();
            if ((hor != 0) || (ver != 0))
            {
                scroll(
                    hor * Time.deltaTime * scrollModifier,
                    ver * Time.deltaTime * scrollModifier);

                return;
            }

            /*
            //Edge map scrolling
            if (Common.mouseInsideGameWindow())
            {
                if (Input.mousePosition.x < scrollingSettings.edgeScrollPixels)
                    scroll(Time.deltaTime * -scrollModifier, 0);
                if (Input.mousePosition.x > (Screen.width - scrollingSettings.edgeScrollPixels))
                    scroll(Time.deltaTime * scrollModifier, 0);
                if (Input.mousePosition.y < scrollingSettings.edgeScrollPixels)
                    scroll(0, Time.deltaTime * -scrollModifier);
                if (Input.mousePosition.y > (Screen.height - scrollingSettings.edgeScrollPixels))
                    scroll(0, Time.deltaTime * scrollModifier);
            }
            */
        }
    }
    void setCameraPosition(float x, float y)
    {
        /*
        //Validate new position
        if (x < scrollingSettings.contraints.horizontal.x)
            x = scrollingSettings.contraints.horizontal.x;
        else
            if (x > scrollingSettings.contraints.horizontal.y)
                x = scrollingSettings.contraints.horizontal.y;

        if (y < scrollingSettings.contraints.vertical.x)
            y = scrollingSettings.contraints.vertical.x;
        else
            if (y > scrollingSettings.contraints.vertical.y)
                y = scrollingSettings.contraints.vertical.y;
        */
        //Update current position with new position
        //camTrans.localPosition = new Vector3(x, y, camTrans.localPosition.z);
        camTrans.localPosition = new Vector3(x, camTrans.localPosition.y, y);
    }
    
    public void zoom(float deltaDistance)
    {
        if(deltaDistance != 0)
        {
            float newSize = mapCam.orthographicSize + (-deltaDistance * Time.deltaTime * scrollingSettings.zoomSpeed);
            mapCam.orthographicSize =
                Mathf.Min(
                    Mathf.Max(newSize, scrollingSettings.zoomConstraints.x),
                    scrollingSettings.zoomConstraints.y);
        }
    }
    public void scroll(float hor, float ver)
    {
        if ((hor != 0) || (ver != 0))
        {
            //Limit maximum scroll distance
            hor = Mathf.Min(hor, scrollingSettings.mouseSpeedLimit);
            ver = Mathf.Min(ver, scrollingSettings.mouseSpeedLimit);

            //Cache new position
            float newX = camTrans.localPosition.x + (hor * scrollingSettings.scrollSpeed);
            //float newY = camTrans.localPosition.y + (ver * scrollingSettings.scrollSpeed);
            float newY = camTrans.localPosition.z + (ver * scrollingSettings.scrollSpeed);

            setCameraPosition(newX, newY);
        }
    }
    public void scrollTo(Vector3 focalPoint)
    {
        setCameraPosition(focalPoint.x, focalPoint.z);
    }
    public void centerOnPlayer()
    {
        scrollTo(playerMarkerTrans.position);
    }
    
    //Event Handlers
    void OnMapAreaPointerEnter()
    {
        allowMouseClick = true;
    }
    void OnMapAreaPointerExit()
    {
        allowMouseClick = false;
    }
    public void OnPositionClicked(Vector3 mousePosition)
    {
        startTravellingTo(mapCam.ScreenToWorldPoint(mousePosition));
    }
    public void OnPlayerDataChanged()
    {
        updateFromPlayerData();
    }
    void updatePlayerData()
    {
        GameDatabase.sPlayerData.worldPosition = playerMarkerTrans.position;
        GameDatabase.sPlayerData.fogOfWarState = fogOfWar.getState();
    }
    void updateFromPlayerData()
    {
        //Load Fog Of War state
        fogOfWar.setState(GameDatabase.sPlayerData.fogOfWarState);

        //Load player's map position
        playerMarker.teleport(GameDatabase.sPlayerData.worldPosition);

        //Center Camera on new position
        centerOnPlayer();
    }

    void setupLocations()
    {
        for (int i = 0; i < guiMapLocations.Length; i++)
        {
            //Setup location states based on quest objectives and clearance flags
            guiMapLocations[i].showMarker(false);

            //List<Missions.rowIds> missionList = new List<Missions.rowIds>();

            /*
            for (i = 0; i < DB_Missions.instance.recs.Count; i++)
            {
                if ((DB_Missions.instance.recs[i]._mapLocation == currentLocation.ToString()) &&
                    (isMissionVisible((Missions.rowIds)i)))
                {

                }
            }
            */

        }
        updateLocations();
    }
    void updateLocations()
    {
        float dist;
        for (int i = 0; i < guiMapLocations.Length; i++)
        {
            //Marker toggling for locations in the distance
            dist = Vector3.Distance(guiMapLocations[i].transform.position, playerMarkerTrans.position);
            Debug.Log("Distance to " + guiMapLocations[i].name + " is " + dist);
            guiMapLocations[i].showMarker(dist > 24f);
        }
    }
}