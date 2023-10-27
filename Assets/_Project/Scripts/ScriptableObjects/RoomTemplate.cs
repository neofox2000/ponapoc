using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[CreateAssetMenu(menuName = "Templates/Room")]
public class RoomTemplate : ScriptableObject
{
    [Header("Lighting")]
    public PostProcessProfile postProcessingProfile;
    public Color personalLightColor;

    [Header("Music")]
    public MusicTrackTemplate normalMusic;
    public MusicTrackTemplate battleMusic;

    public void PlayNormalMusic()
    {
        AudioManager.instance.PlayMusic(new AudioManager.TrackRequest(normalMusic));
    }
    public void PlayBattleMusic()
    {
        AudioManager.instance.PlayMusic(new AudioManager.TrackRequest(battleMusic));
    }
}