using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Race")]
public class RaceTemplate : ScriptableObject
{
    public int saveID;
    public Color32 color;
    public bool playable = false;
    [TextArea(3, 10)]
    public string description;
}