using UnityEngine;

namespace RPGData
{
    [System.Serializable]
    public struct StatRule
    {
        [Tooltip("Which stat will be modified by this rule")]
        public StatTypes affectedStat;
        [Tooltip("This value is added directly to the `Affected Stat` for each point of the stat with this rule")]
        public float flatValue;
        [Tooltip("This value is multiplied by - and then added to - the `Affected Stat` for each point of the stat with this rule")]
        [UnityEngine.Serialization.FormerlySerializedAs("magnitudePerPoint")]
        public float percentage;
        [Tooltip("Only items with this tag will be affected")]
        public ItemTag affectedItemTag;

        public float GetAmountToAdd(float statTarget, float statSource)
        {
            return
                (flatValue +
                ((percentage / 100f) * statTarget))
                * statSource;
        }
    }
}