using UnityEngine;
using RPGData;

namespace Fungus
{
    /// <summary>
    /// Puts the game into a paused state and sets the SayDialog
    /// </summary>
    [CommandInfo("Custom",
                 "Get Player Attribute",
                 "Stores the selected player attribute into the specified variable")]
    [AddComponentMenu("")]
    public class GetPlayerAttribute : Command
    {
        [Tooltip("Attribute to look for in the player's character sheet")]
        [SerializeField] protected AttributeTemplate attributeTemplate;
        [Tooltip("Variable in which to store the current attribute value")]
        [VariableProperty(typeof(FloatVariable))]
        [SerializeField] protected FloatVariable floatVariable;

        #region Public members

        public override void OnEnter()
        {
            if (floatVariable) floatVariable.Value = 
                    GameDatabase.lCharacterSheet
                        .GetAttribute(attributeTemplate)
                        .valueModded;
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }
        
        #endregion
    }
}