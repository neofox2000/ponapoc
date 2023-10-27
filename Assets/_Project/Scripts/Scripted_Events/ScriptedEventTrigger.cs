using UnityEngine;
using System.Collections;

public class ScriptedEventTrigger : MonoBehaviour
{
    public enum ScriptedEventActionType { None, EnableObject, DisableObject, AnimationTrigger }

    [System.Serializable]
    public class ScriptedEventAction
    {
        public ScriptedEventActionType actionType;
        public Transform target;
    }

    [System.Serializable]
    public class ScriptedEvent
    {
        public string name;
        public ScriptedEventAction[] actions;
    }

    public ScriptedEvent[] events;

    void execEvent(ScriptedEvent _event)
    {
        foreach(ScriptedEventAction action in _event.actions)
        {
            switch(action.actionType)
            {
                case ScriptedEventActionType.EnableObject:
                    action.target.gameObject.SetActive(true);
                    break;
                case ScriptedEventActionType.DisableObject:
                    action.target.gameObject.SetActive(false);
                    break;
                case ScriptedEventActionType.AnimationTrigger:
                    AnimationTrigger at = action.target.GetComponent<AnimationTrigger>();
                    if (at) at.OnTriggered(); else Debug.LogWarning("No Animation Trigger found!");
                    break;
            }
        }
    }
    void OnScriptedEvent(string eventName)
    {
        foreach (ScriptedEvent _event in events)
            if (_event.name == eventName)
                execEvent(_event);
    }
}
