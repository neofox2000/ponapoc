// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu]
    public class GameEvent : ScriptableObject
    {
        public bool debug = false;
        [TextArea(3, 10)] public string comment;

        /// <summary>
        /// The list of listeners that this event will notify if it is raised.
        /// </summary>
        private readonly List<GameEventListener> eventListeners = 
            new List<GameEventListener>();

        public void Raise()
        {
            if (debug) Debug.Log(name + " GameEvent was raised - " + eventListeners.Count + " listeners.");
            for(int i = eventListeners.Count -1; i >= 0; i--)
                eventListeners[i].OnEventRaised();
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (debug) Debug.Log(listener.name + " regsitered as listener to " + name);
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (debug) Debug.Log(listener.name + " unregsitered from " + name);
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}