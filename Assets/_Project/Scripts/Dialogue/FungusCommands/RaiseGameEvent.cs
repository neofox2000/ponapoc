using Events;
using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Writes text in a dialog box.
    /// </summary>
    [CommandInfo("Scripting",
                 "GameEvent",
                 "Raises the assigned GameEvent")]
    [AddComponentMenu("")]
    public class RaiseGameEvent : Command
    {
        [SerializeField]
        protected GameEvent gameEvent;

        public override void OnEnter()
        {
            if (gameEvent)
                gameEvent.Raise();
            else
                Debug.LogError("No GameEvent assigned to this Command Block");
            Continue();
        }
        public override string GetSummary()
        {
            if (gameEvent)
                return gameEvent.name + " (GameEvent)";
            else
                return "<Assign a GameEvent>";
        }
        public override Color GetButtonColor()
        {
            return Color.blue;
        }
    }
}