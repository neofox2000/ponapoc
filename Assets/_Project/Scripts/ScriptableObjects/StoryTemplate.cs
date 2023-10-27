using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Story Template")]
public class StoryTemplate : ScriptableObject
{
    public enum StoryActions { None, Pause, UnPause, LoadStoryTeller, LoadTitle, LoadMission }

    public Color32 headingTextColor, bodyTextColor;
    public StoryActions before, after;
    public string heading;
    [TextArea(10, 100)]
    public string body;
}