using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Quest")]
public class QuestTemplate : ScriptableObject
{
    public int saveID;

    public float timeLimit;
    [TextArea(3, 10)]
    public string description;
    [TextArea(3, 10)]
    public string successDescription;
    [TextArea(3, 10)]
    public string failureDescription;
}