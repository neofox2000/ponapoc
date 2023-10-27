using UnityEngine;
using System.Collections.Generic;

namespace GameDB
{
    public class StatEffectArea : MonoBehaviour, IStatusEffectSource
    {
        public const string msg_StatEffectAreaEntered = "OnStatEffectAreaEntered";
        public const string msg_StatEffectAreaExited = "OnStatEffectAreaExited";

        public StatusEffectTemplate statusEffect;

        //IStatusEffectSource Methods
        StatusEffectTemplate IStatusEffectSource.GetTemplate()
        {
            return statusEffect;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            collider.SendMessage(
                msg_StatEffectAreaEntered, 
                this, 
                SendMessageOptions.DontRequireReceiver);
        }
        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            collider.SendMessage(
                msg_StatEffectAreaExited, 
                this, 
                SendMessageOptions.DontRequireReceiver);
        }
    }
}