using UnityEngine;
using System;

[Serializable]
public struct DamageResult
{
    public float value;
    public bool crit;
}
[Serializable]
public struct HitData
{
    //Is this damage caused by a player?
    public bool fromPlayer;

    //Array of damage types and their flat values
    public ForceMatrix damage;

    //Any extra damage modifiers derived from external systems/mechanics
    public float damageModifier;

    //Chance (0 to 100) to critical hit something
    public float critChance;

    //How much more damage a critical hit will do
    public float critDamageModifier;

    //The world position where the hit (collision) occurred
    public Vector3 hitPosition;

    //Reference to the hitbox involved (if available)
    public HitBox hitBox;

    /// <summary>
    /// Cached damage result when compared against target's protection
    /// </summary>
    public DamageResult damageResult;

    public HitData(bool fromPlayer, ForceMatrix damage, float damageModifier, float critChance, float critDamageModifier, Vector3 hitPosition, HitBox hitBox, DamageResult damageResult)
    {
        this.fromPlayer = fromPlayer;
        this.damage = damage;
        this.damageModifier = damageModifier;
        this.critChance = critChance;
        this.critDamageModifier = critDamageModifier;
        this.hitPosition = hitPosition;
        this.hitBox = hitBox;
        this.damageResult = new DamageResult();
    }

    public void CalculateDamage(ForceMatrix targetProtection)
    {
        //Roll for critical hit success
        damageResult.crit = critChance >= UnityEngine.Random.Range(0f, 100f);

        //Calculate damage (don't allow negative damage numbers)
        damageResult.value = Mathf.Max(0f,
            //Overriding modifier
            damageModifier *
            //Resistance of the target
            ((1f - targetProtection) * damage).Sum() *
            //Hitbox modifier (eg: headshots generally do more damage)
            (hitBox == null ? 1f : hitBox.damageMod) *
            //Critical hit modifier
            (damageResult.crit ? critDamageModifier : 1f)
        );

        //If damage is very little, round it to either 1 or 0 to avoid problems
        if ((damageResult.value < 1f) && (damageResult.value > 0f)) Mathf.Round(damageResult.value);
    }
}

[Serializable]
public class HitBox : MonoBehaviour 
{
    //Constant
    public const string msgOnAttacked = "onAttacked";

    //Inspector Properties
    public string bodyPartName;
    public float damageMod = 1f;

    //Events
    public Action<HitData> OnHit;

    //Accessors
    public int team { get; set; }
    public Actor controller { get; set; }

    void onAttacked(HitData hitInfo)
    {
        hitInfo.hitBox = this;

        if (OnHit != null)
            OnHit(hitInfo);
        else
            SendMessageUpwards("OnAttacked", hitInfo);
    }
}
