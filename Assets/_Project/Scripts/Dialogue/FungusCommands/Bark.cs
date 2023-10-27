using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Writes text in a dialog box.
    /// </summary>
    [CommandInfo("Custom",
                 "Bark",
                 "Barks text in a floating text box.")]
    [AddComponentMenu("")]
    public class Bark : Command, ILocalizable
    {
        [TextArea(5, 10)]
        [SerializeField]
        protected string storyText = "";

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField]
        protected string description = "";

        [Tooltip("Variable to store text from this block that will be picked up by the Dialogue Manager for displaying the bark")]
        [VariableProperty(typeof(StringVariable))]
        [SerializeField]
        protected Variable outputTextVariable;

        #region Public members
        public override void OnEnter()
        {
            var flowchart = GetFlowchart();
            string displayText = storyText;
            StringVariable outString = outputTextVariable as StringVariable;
            outString.Value = string.Empty;

            var activeCustomTags = CustomTag.activeCustomTags;
            for (int i = 0; i < activeCustomTags.Count; i++)
            {
                var ct = activeCustomTags[i];
                displayText = displayText.Replace(ct.TagStartSymbol, ct.ReplaceTagStartWith);
                if (ct.TagEndSymbol != "" && ct.ReplaceTagEndWith != "")
                {
                    displayText = displayText.Replace(ct.TagEndSymbol, ct.ReplaceTagEndWith);
                }
            }

            //Copy text to variable
            outString.Value = flowchart.SubstituteVariables(displayText);

            Continue();
        }
        public override string GetSummary()
        {
            return "\"" + storyText + "\"";
        }
        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }
        #endregion

        #region ILocalizable implementation
        public virtual string GetStandardText()
        {
            return storyText;
        }
        public virtual void SetStandardText(string standardText)
        {
            storyText = standardText;
        }
        public virtual string GetDescription()
        {
            return description;
        }
        public virtual string GetStringId()
        {
            // String id for Bark commands is BARK.<Localization Id>.<Command id>
            return "BARK." + GetFlowchartLocalizationId() + "." + itemId;
        }
        #endregion
    }
}