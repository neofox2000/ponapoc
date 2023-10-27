using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Templates/Weapon Template")]
public class WeaponTemplate : ScriptableObject
{
    public enum FireModes { Manual, Auto }
    public enum WeaponTypes { Instant, Projectile, Mixed, Melee }
    public enum ReloadMethods { WholeClip = 0, SingleRound = 1 }
    public enum EjectionMethods { OnFireOnce, OnFireAll, OnReload }

    [Serializable]
    public struct WeaponSounds
    {
        public AudioGroupTemplate fire, empty, reloadStart, reloadMid, reloadEnd;
    }

    public FireModes fireMode;
    public WeaponTypes weaponType;
    public ReloadMethods reloadMethod = ReloadMethods.WholeClip;
    public EjectionMethods ejectionMethod = EjectionMethods.OnFireOnce;

    public int shotCount = 1;
    public float shotSpread;
    public float pieceringFactor;

    public WeaponSounds sounds;
    public Vector3[] attackEmitters;
    public CommonStructures.EffectSpawnRelative[] fireEffects;
    public CommonStructures.EffectSpawnRelative[] ejectionEffects;
    public CommonStructures.EffectSpawnRelative[] tracerEffects;
    public CommonStructures.EffectSpawnRelative[] projectiles;
}