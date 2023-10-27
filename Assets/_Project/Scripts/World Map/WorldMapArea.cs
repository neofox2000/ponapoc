using UnityEngine;

public class WorldMapArea : MonoBehaviour 
{
    public int[] encounters;

    void Awake()
    {
        gameObject.tag = "MapArea";
    }
}
