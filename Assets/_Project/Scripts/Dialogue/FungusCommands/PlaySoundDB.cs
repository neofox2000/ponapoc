using UnityEngine;
using GameDB;

namespace Fungus
{
    /// <summary>
    /// Plays a sound from our database
    /// </summary>
    [CommandInfo("Audio",
                 "Play Sound DB",
                 "Plays a sound from our database")]
    [AddComponentMenu("")]
    public class PlaySoundDB : Command
    {
        [Tooltip("Sound to play")]
        [SerializeField] protected AudioGroupTemplate sound;

        protected virtual void DoWait()
        {
            Continue();
        }

        #region Public members

        public override void OnEnter()
        {
            AudioManager.instance.Play(sound);

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}
