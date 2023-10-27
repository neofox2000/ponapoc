using UnityEngine;
using Variables;

namespace RPGData
{
    [CreateAssetMenu(menuName = "Templates/Attribute")]
    public class AttributeTemplate : ScriptableObject
    {
        [Tooltip("ID to use in the serialization process when saving game")]
        public int saveID;
        [Tooltip("Mostly for differentiation purposes in editor")]
        public bool isSkill;
        [Tooltip("The name to display to the player in their character sheet\n(Uses the object's name if left blank)")]
        [SerializeField] string displayName;
        [Tooltip("Description to display to the player in their character sheet")]
        [TextArea(2, 10)]
        public string description;
        [Tooltip("Font color when displaying this stat's name")]
        public Color32 color;
        public StringVariable unitOfMeasure;
        [Tooltip("Upper and lower limits")]
        public Vector2 limits;
        [Tooltip("How the stats are affected")]
        public StatRule[] statRules;

        public string GetDisplayName()
        {
            return displayName == string.Empty ? name : displayName;
        }
    }
}