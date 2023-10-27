using UnityEngine;
using System.Collections.Generic;
using GameDB;

public class GUI_LocationPanel : MonoBehaviour
{
    //Properties
    public RectTransform missionsContainer;
    public GameObject missionWidgetPrefab;    
    public WorldLocationTemplate currentLocation;

    bool locationChosen = false;
    WorldLocationEntrance selectedEntryPoint;
    List<GUI_MissionButton> buttons = new List<GUI_MissionButton>();

    #region Event Handlers
    public void OnEnterLocation(WorldLocationTemplate locationID)
    {
        currentLocation = locationID;
        SetupMissions(currentLocation);
    }
    public void OnExitLocation()
    {
        locationChosen = false;
        SetupMissions(null);
    }
    public void OnStartMissionClick()
    {
        if(locationChosen)
            GUIManager.instance.guiWorldMap.OnEnterLocation(selectedEntryPoint);
    }
    public void OnMissionClicked(int buttonIndex)
    {
        //resetButtons();
        selectedEntryPoint = currentLocation.entrances[buttonIndex];
        locationChosen = true;
        GUIManager.instance.guiWorldMap.OnEnterLocation(selectedEntryPoint);
    }
    #endregion

    #region Methods
    GUI_MissionButton CreateMissionWidget()
    {
        GameObject GO = null;
        GO = Instantiate(missionWidgetPrefab) as GameObject;
        RectTransform rTrans = GO.GetComponent<RectTransform>();
        rTrans.SetParent(missionsContainer);

        //Correct Unity bug that alters scale
        rTrans.localScale = Vector3.one;

        buttons.Add(GO.GetComponent<GUI_MissionButton>());
        return (buttons[buttons.Count - 1]);
    }
    void SetupMissions(WorldLocationTemplate location)
    {
        GUI_MissionButton button = null;
        int c = 0;
        locationChosen = false;

        //Hide All Buttons
        foreach (GUI_MissionButton b in buttons) b.hide();

        //Setup buttons
        if (location != null)
        {
            for (int i = 0; i < location.entrances.Length; i++)
            {
                if (GameDatabase.sPlayerData.GetLocationVisible(location.saveId, location.entrances[i].saveId))
                {
                    c++;
                    if (buttons.Count < c)
                        button = CreateMissionWidget();
                    else
                        button = buttons[c - 1];

                    //entryPoints.Add(ep);
                    button.show(OnMissionClicked, c - 1, location.entrances[i].displayName);
                }
            }
        }

        //Select first button (if available)
        buttons[0].selected = buttons[0].gameObject.activeSelf;
    }
    #endregion
}