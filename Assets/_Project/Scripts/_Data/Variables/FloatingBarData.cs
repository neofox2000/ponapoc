using UnityEngine;
using UnityEngine.Events;

namespace Variables
{
    /// <summary>
    /// Typically should only need one.
    /// I can't currently think of any scenarios where more than 1 progress bar would be needed
    /// But if this does change, it's just a matter of creating and linking more to multiple data pluggers
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Floating Bar Data")]
    public class FloatingBarData : ScriptableObject
    {
        [Header("Style")]
        public string displayText;
        //public Color textColor;
        public Color fillColor;
        public bool showValueText;
        public bool showMaxValue;

        [Header("Position")]
        public Transform target;
        public Vector3 fixedPosition;
        public Vector3 offset;

        [Header("Useage")]
        bool inUse = false;
        public float currentProgress = 0f;
        public float maxProgress = 1f;

        //Events
        public UnityEvent OnInUseChanged;
        public void SetUsing(bool inUse)
        {
            this.inUse = inUse;

            OnInUseChanged.Invoke();
        }
        public bool GetInUse()
        {
            return inUse;
        }
    }
}