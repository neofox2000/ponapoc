using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Templates/Character Sounds")]
public class CharacterSoundsTemplate : ScriptableObject
{
    public enum SoundKeys
    {
        None,
        npcGreeting,
        npcFairwell,
        npcBarterAccepted,
        npcBarterCanceled,
        npcHaggleSuccess,
        npcHaggleFailure,
        npcHaggleExhausted,
        npcBarterRejected,
        npcBarterStarted,
        aiStateAttack,
        aiStateSeek,
        aiStateIdle,
        hurt
    }

    [Serializable]
    public struct CharacterSound
    {
        public string name;
        public AudioGroupTemplate sound;
        public SoundKeys key;
    }

    public CharacterSound[] sounds;
}