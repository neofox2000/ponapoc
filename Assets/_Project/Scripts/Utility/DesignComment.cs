#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DesignComment : MonoBehaviour
{
    public string comment;

    private void OnDrawGizmos()
    {
        if(comment != string.Empty)
            Handles.Label(transform.position, comment);
    }
}
#endif