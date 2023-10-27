using UnityEngine;
using GameDB;

namespace Fungus
{
    /// <summary>
    /// Sets the visibility on a world map loation's entry point
    /// </summary>
    [CommandInfo("Quests",
                 "Set Location Visibility",
                 "Sets the visibility of a location on the world map")]
    [AddComponentMenu("")]
    public class SetLocationVisibility : Command
    {
        [Tooltip("World Location")]
        [SerializeField] protected WorldLocationEntrance entrance;

        [Tooltip("New Visibility")]
        [SerializeField] protected bool visible;

        #region Public members

        public override void OnEnter()
        {
            if(entrance == null)
            {
                Debug.LogWarning("Entrance property not set on Fungus SetLocationVisibility command!");
                Continue();
                return;
            }

            GameDatabase.sPlayerData.SetLocationVisible(
                entrance.parentLocation.saveId,
                entrance.saveId,
                visible);

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(80, 175, 185, 255);
        }

        public override string GetSummary()
        {
            string s = "Set Entrance";
            if (entrance != null)
                return s += " " + entrance.displayName + " to " + (visible ? "Visible" : "Hidden");

            return s;
        }
        #endregion
    }
}