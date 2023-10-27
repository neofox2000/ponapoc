using UnityEngine;
using System.Collections;

public class ScriptScrubber : MonoBehaviour 
{
    void Awake()
    {
        MonoBehaviour[] comps;

        comps = GetComponentsInChildren<MonoBehaviour>();
        
        foreach (MonoBehaviour m in comps)
            Destroy(m);

        comps = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour m in comps)
            Destroy(m);
    }
}
