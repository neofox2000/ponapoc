using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Puts the game into a paused state and sets the SayDialog
    /// </summary>
    [CommandInfo("Narrative",
                 "End Conversation",
                 "Unpauses the game")]
    [AddComponentMenu("")]
    public class EndConversation : Command
    {
        #region Public members

        public override void OnEnter()
        {
            //DialogueManager.instance.EndConversation();
            Debug.LogError("Old EndConversation Command!  Use GameEvent instead!");
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(255, 80, 80, 255);
        }
        #endregion
    }
}