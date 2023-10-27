using UnityEngine;

public class MultiObjectTrigger : MonoTrigger
{
    public enum GameObjectAction { Enable, Disable, Destroy }

    [System.Serializable]
    public struct TriggerAction
    {
        public string name;
        public GameObjectAction action;
        public GameObject gameObject;
    }

    public TriggerAction[] triggerActions;

    protected override void fire()
    {
        base.fire();

        foreach (var ta in triggerActions)
        {
            switch(ta.action)
            {
                case GameObjectAction.Enable:
                    ta.gameObject.SetActive(true);
                    break;
                case GameObjectAction.Disable:
                    ta.gameObject.SetActive(false);
                    break;
                case GameObjectAction.Destroy:
                    Destroy(ta.gameObject);
                    break;
            }
        }
    }
}