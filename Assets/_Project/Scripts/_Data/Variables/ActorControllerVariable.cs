using UnityEngine;

namespace Variables
{
    [CreateAssetMenu(menuName = "Data/Actor Controller Variable")]
    public class ActorControllerVariable : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        public ActorController Value;

        public System.Action<ActorController, ActorController> BeforeValueChanged;

        public void SetValue(ActorController value)
        {
            //Notify of change from Old to New values
            BeforeValueChanged?.Invoke(Value, value);

            //Make change
            Value = value;
        }

    }
}