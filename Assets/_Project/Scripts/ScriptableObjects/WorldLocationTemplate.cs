using UnityEngine;

[CreateAssetMenu(menuName = "Templates/World Location Template")]
public class WorldLocationTemplate : ScriptableObject
{
    [System.Serializable]
    public struct Entrance
    {
        [Tooltip("Displayed if shown on the map GUI")]
        public string name;

        [Tooltip("ID of the entrace portal in the level prefab")]
        public int portalID;

        [Tooltip("Determines if this location will be visible at the start of a new game")]
        public bool newGameVisible;
    }

    public int saveId;
    [TextArea]
    public string description;
    public string sceneName;
    public WorldLocationEntrance[] entrances;

    public class SortBySaveID : System.Collections.Generic.Comparer<WorldLocationTemplate>
    {
        public override int Compare(WorldLocationTemplate x, WorldLocationTemplate y)
        {
            return x.saveId.CompareTo(y.saveId);
        }
    }
}