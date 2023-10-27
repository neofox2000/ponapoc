using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Puts the game into a paused state and sets the SayDialog
    /// </summary>
    [CommandInfo("Barter",
                 "Start Barter Session",
                 "Puts the game into a paused state and sets up the barter GUI")]
    [AddComponentMenu("")]
    public class StartBarter : Command
    {
        #region Public members

        public override void OnEnter()
        {
            //DialogueManager.instance.StartBarter();
            Debug.LogError("Old StartBarter Command!  Use GameEvent instead!");
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(95, 165, 33, 255);
        }
        #endregion
    }
}