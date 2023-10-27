using UnityEngine;

namespace Variables
{
    [CreateAssetMenu(menuName = "Data/Floating Text Request")]
    public class FloatingTextRequest : ScriptableObject
    {
        public string text;
        public Color color;
        public Transform target;
        public Vector3 fixedPosition;

        void Set(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }
        public void Set(string text, Color color, Transform target)
        {
            Set(text, color);
            this.target = target;
        }
        public void Set(string text, Color color, Vector3 fixedPosition)
        {
            Set(text, color);
            this.fixedPosition = fixedPosition;
            target = null;
        }
    }
}