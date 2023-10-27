using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Templates/Weapon Animation Profile")]
public class WeaponAnimationProfile : ScriptableObject
{
    [Serializable]
    public struct WeaponAnimation
    {
        public ItemTemplate weaponItem;
        [Tooltip("Determines which aim, attack and reload animations to use for the chosen weapon")]
        public int animationKey;
    }

    public WeaponAnimation[] weaponAnimations;

    public int GetAnimationKey(ItemTemplate item)
    {
        foreach (WeaponAnimation wc in weaponAnimations)
            if (wc.weaponItem == item)
                return wc.animationKey;

        return 0;
    }
}