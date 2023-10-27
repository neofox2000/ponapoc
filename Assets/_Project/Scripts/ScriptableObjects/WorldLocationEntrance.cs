using UnityEngine;

[CreateAssetMenu(menuName = "Templates/World Location Entrance")]
public class WorldLocationEntrance : ScriptableObject
{
    public int saveId;

    [Tooltip("Displayed when shown on the world map GUI")]
    public string displayName;

    [Tooltip("The world location that this entrance belongs to")]
    public WorldLocationTemplate parentLocation;

    [Tooltip("Determines if this location will be visible at the start of a new game")]
    public bool newGameVisible;
}