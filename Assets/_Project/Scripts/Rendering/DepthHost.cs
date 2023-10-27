using UnityEngine;

public class DepthHost : MonoBehaviour
{
    [Tooltip("Fixed scale offset for all characters")]
    public float scaleOffset;
    [Tooltip("How much scale changes as characters move through the simulated depth")]
    public float scaleFator;

    public static DepthHost Find(Transform target)
    {
        DepthHost host = null;
        Collider2D[] colliders = Physics2D.OverlapPointAll(target.position);
        foreach (Collider2D col in colliders)
        {
            host = col.GetComponent<DepthHost>();
            if (host) return host;
        }

        //No host found
        return null;
    }
}