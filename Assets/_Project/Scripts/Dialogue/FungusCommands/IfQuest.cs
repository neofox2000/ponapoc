using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// If the test expression is true, execute the following command block.
    /// </summary>
    [CommandInfo("Quests",
                 "If Quest",
                 "If the Quest is at the specified state, execute the following command block.")]
    [AddComponentMenu("")]
    public class IfQuest : VariableCondition
    {
        [Tooltip("Old quest save ID (ignore this)")]
        [SerializeField] protected int quest;
        [Tooltip("Quest to check")]
        [SerializeField] protected QuestTemplate _quest;

        [Tooltip("State to check for")]
        [SerializeField] protected QuestState state;

        protected override void EvaluateAndContinue()
        {
            if (EvaluateCondition())
            {
                OnTrue();
            }
            else
            {
                OnFalse();
            }
        }

        protected override void OnTrue()
        {
            Continue();
        }

        protected override void OnFalse()
        {
            // Last command in block
            if (CommandIndex >= ParentBlock.CommandList.Count)
            {
                StopParentBlock();
                return;
            }

            // Find the next Else, ElseIf or End command at the same indent level as this If command
            for (int i = CommandIndex + 1; i < ParentBlock.CommandList.Count; ++i)
            {
                Command nextCommand = ParentBlock.CommandList[i];

                if (nextCommand == null)
                {
                    continue;
                }

                // Find next command at same indent level as this If command
                // Skip disabled commands, comments & labels
                if (!((Command)nextCommand).enabled ||
                    nextCommand.GetType() == typeof(Comment) ||
                    nextCommand.GetType() == typeof(Label) ||
                    nextCommand.IndentLevel != indentLevel)
                {
                    continue;
                }

                System.Type type = nextCommand.GetType();
                if (type == typeof(Else) ||
                    type == typeof(End))
                {
                    if (i >= ParentBlock.CommandList.Count - 1)
                    {
                        // Last command in Block, so stop
                        StopParentBlock();
                    }
                    else
                    {
                        // Execute command immediately after the Else or End command
                        Continue(nextCommand.CommandIndex + 1);
                        return;
                    }
                }
                else if (type == typeof(ElseIfQuest))
                {
                    // Execute the Else If command
                    Continue(i);

                    return;
                }
            }

            // No matching End command found, so just stop the block
            StopParentBlock();
        }

        protected override bool EvaluateCondition()
        {
            bool condition = false;
            QuestState questState = GameDatabase.sPlayerData.GetQuestState(_quest);
            switch(compareOperator)
            {
                case CompareOperator.Equals:
                case CompareOperator.GreaterThanOrEquals:
                case CompareOperator.LessThanOrEquals:
                    condition = questState == state;
                    break;
                case CompareOperator.NotEquals:
                case CompareOperator.GreaterThan:
                case CompareOperator.LessThan:
                    condition = questState != state;
                    break;
            }

            return condition;
        }

        protected override bool HasNeededProperties()
        {
            return _quest != null;
        }

        #region Public members

        public override void OnEnter()
        {
            if (ParentBlock == null)
            {
                return;
            }

            EvaluateAndContinue();
        }

        public override string GetSummary()
        {
            string summary = "Quest: " + (_quest ? _quest.name : "<none>") + " ";
            summary += Condition.GetOperatorDescription(compareOperator) + " ";
            summary += state.ToString();
            return summary;
        }

        public override bool OpenBlock()
        {
            return true;
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion
    }
}