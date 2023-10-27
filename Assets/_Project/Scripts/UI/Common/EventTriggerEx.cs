using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventTriggerEx : MonoBehaviour
{
    public UnityEvent Enabled;
    public UnityEvent Disabled;

    private void OnEnable()
    {
        Enabled.Invoke();
    }
    private void OnDisable()
    {
        Disabled.Invoke();
    }
}
