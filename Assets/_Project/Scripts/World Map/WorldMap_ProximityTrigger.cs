using UnityEngine;
using GameDB;

public class WorldMap_ProximityTrigger : MonoBehaviour
{
    #region Structures
    public enum TriggerActionTypes { 
        StopTravel, 
        TravelTo, 
        TurnOffBlocker, 
        TurnOnBlocker, 
        ShowMessage };

    [System.Serializable]
    public class AlertTriggerAction
    {
        public float duration;
        public string text;
    }
    [System.Serializable]
    public class TriggerActions
    {
        public TriggerActionTypes actionType = TriggerActionTypes.StopTravel;
        public Vector3 travelDestination;
        public int triggerCount;
        public AlertTriggerAction alert;
    }
    [System.Serializable]
    public class TriggerGroup
    {
        public string name;
        public QuestStatePair[] requiredQuests;
        public TriggerActions[] triggerActions;
    }
    #endregion

    //public TriggerActions[] triggerActions;
    public TriggerGroup[] triggers;

    int triggerCount = 0;

    public void fire(GameObject messageReceiver)
    {
        UnityEngine.AI.NavMeshObstacle nmObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        for (int j = 0; j < triggers.Length; j++)
        {
            if (DialogueQuery.areQuestRequirementsMet(triggers[j].requiredQuests))
            for (int i = 0; i < triggers[j].triggerActions.Length; i++)
            {
                if ((triggers[j].triggerActions[i].triggerCount == 0) || (triggerCount < triggers[j].triggerActions[i].triggerCount))
                {
                    //Execute all actions
                    switch (triggers[j].triggerActions[i].actionType)
                    {
                        case TriggerActionTypes.StopTravel:
                            messageReceiver.SendMessage("stopTravelling", true);
                            break;
                        case TriggerActionTypes.TravelTo:
                            messageReceiver.SendMessage("startTravellingTo", triggers[j].triggerActions[i].travelDestination);
                            break;
                        case TriggerActionTypes.TurnOffBlocker:
                            if (nmObstacle)
                                nmObstacle.enabled = false;
                            break;
                        case TriggerActionTypes.TurnOnBlocker:
                            if (nmObstacle)
                                nmObstacle.enabled = true;
                            break;
                        case TriggerActionTypes.ShowMessage:
                            messageReceiver.SendMessage("showAlert", triggers[j].triggerActions[i].alert);
                            break;
                    }
                }
            }
        }

        triggerCount++;
    }
}
