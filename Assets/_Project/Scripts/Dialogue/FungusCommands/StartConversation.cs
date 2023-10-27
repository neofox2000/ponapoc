using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Puts the game into a paused state and sets the SayDialog
    /// </summary>
    [CommandInfo("Narrative",
                 "Start Conversation",
                 "Puts the game into a paused state and sets up the conversation GUI")]
    [AddComponentMenu("")]
    public class StartConversation : Command
    {
        #region Public members

        public override void OnEnter()
        {
            //DialogueManager.instance.StartConversation();
            Debug.LogError("Old StartConversation Command!  Use GameEvent instead!");
            Continue();
        }

        public override Color GetButtonColor()
        {
            return Color.blue;
            //return new Color32(184, 210, 235, 255);
        }
        #endregion
    }
}