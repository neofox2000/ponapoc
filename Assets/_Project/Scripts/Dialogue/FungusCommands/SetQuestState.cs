using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Puts the game into a paused state and sets the SayDialog
    /// </summary>
    [CommandInfo("Quests",
                 "Set Quest State",
                 "Sets the state of a quest")]
    [AddComponentMenu("")]
    public class SetQuestState : Command
    {
        [Tooltip("Old quest save ID (ignore this)")]
        [SerializeField] protected int quest;
        [Tooltip("Quest to change")]
        [SerializeField] protected QuestTemplate _quest;

        [Tooltip("New State")]
        [SerializeField] protected QuestState state;

        #region Public members

        public override void OnEnter()
        {
            GameDatabase.sPlayerData.SetQuestState(
                _quest, 
                state, 
                0f);

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(80, 175, 185, 255);
        }

        public override string GetSummary()
        {
            return "Set Quest: " + (_quest ? _quest.name : "<none>") + " to " + state;
        }
        #endregion
    }
}