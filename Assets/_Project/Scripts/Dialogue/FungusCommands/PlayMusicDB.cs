using UnityEngine;
using GameDB;

namespace Fungus
{
    /// <summary>
    /// Plays looping game music from a Music Track Template
    /// </summary>
    [CommandInfo("Audio",
                 "Play Music DB",
                 "Plays looping game music from a Music Track Template")]
    [AddComponentMenu("")]
    public class PlayMusicDB : Command
    {
        [Tooltip("Music Track to play")]
        [SerializeField] protected MusicTrackTemplate musicTrack;

        [Tooltip("Length of time to fade in music.")]
        [SerializeField] protected float fadeInDuration = 1f;
        [Tooltip("Length of time to fade out previous playing music.")]
        [SerializeField] protected float fadeOutDuration = 1f;

        #region Public members

        public override void OnEnter()
        {
            AudioManager.instance.PlayMusic(
                new AudioManager.TrackRequest(musicTrack, fadeInDuration),
                fadeOutDuration);

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}