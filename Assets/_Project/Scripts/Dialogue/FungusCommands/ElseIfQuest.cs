using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.
    /// </summary>
    [CommandInfo("Quests",
                 "Else If Quest",
                 "Marks the start of a command block to be executed when the preceding If Quest statement is False and the test expression is true.")]
    [AddComponentMenu("")]
    public class ElseIfQuest : IfQuest
    {
        #region Public members

        public override void OnEnter()
        {
            System.Type previousCommandType = ParentBlock.GetPreviousActiveCommandType();

            if (previousCommandType == typeof(IfQuest) ||
                previousCommandType == typeof(ElseIfQuest))
            {
                // Else If behaves the same as an If command
                EvaluateAndContinue();
            }
            else
            {
                // Else If behaves mostly like an Else command, 
                // but will also jump to a following Else command.

                // Stop if this is the last command in the list
                if (CommandIndex >= ParentBlock.CommandList.Count - 1)
                {
                    StopParentBlock();
                    return;
                }

                // Find the next End command at the same indent level as this Else If command
                int indent = indentLevel;
                for (int i = CommandIndex + 1; i < ParentBlock.CommandList.Count; ++i)
                {
                    var command = ParentBlock.CommandList[i];

                    if (command.IndentLevel == indent)
                    {
                        System.Type type = command.GetType();
                        if (type == typeof(End))
                        {
                            // Execute command immediately after the Else or End command
                            Continue(command.CommandIndex + 1);
                            return;
                        }
                    }
                }

                // No End command found
                StopParentBlock();
            }
        }

        public override bool OpenBlock()
        {
            return true;
        }

        public override bool CloseBlock()
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