using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Music Track")]
public class MusicTrackTemplate : ScriptableObject
{
    public AudioClip clip;
    [Range(0, 1f)]
    public float volume = 1f;
    [Range(-3f, 3f)]
    public float pitch = 1f;
}