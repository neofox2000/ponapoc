using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Templates/Character Animation Profile")]
public class CharacterAnimationProfile : ScriptableObject
{
    //Structures
    [Serializable]
    public struct AbilityAnimation
    {
        public AbilityTemplate ability;
        [Tooltip("Animation to play when activated")]
        public int animationKey;
    }
    [Serializable]
    public struct ArmorAnimation
    {
        [Tooltip("Animation to use when equipping the chosen armors")]
        public int animationKey;
        [Tooltip("Armors that this animation set will apply to")]
        public ItemTemplate[] armors;
    }
    [Serializable]
    public struct WeaponAnimationSet
    {
        [Tooltip("Which aim, attack and reload animations to use for the chosen weapons")]
        public int animationSet;
        [Tooltip("Weapons that this animation set will apply to")]
        public ItemTemplate[] weapons;
    }

    //Properties
    public AbilityAnimation[] abilityAnimations;
    public ArmorAnimation[] armorAnimations;
    public WeaponAnimationSet[] weaponAnimationSet;
}