using UnityEngine;
using UnityEngine.Rendering;

public class AbilityPayloadController : MonoBehaviour
{
    public enum PayloadType { Generic, Projectile, Turret }

    public PayloadType payloadType;

    //Cached properties (used often)
    public Actor actor { get; set; }
    public BaseAbility ability { get; set; }
    private SortingGroup sortingGroup = null;
    private ForceMatrix offense;

    void InitProjectile()
    {
        //TODO: Convert ability template damage types to the same type used in Init

        GetComponent<Projectile>().Init(
            actor,
            offense,
            0,
            ability.template.speed,
            ability.template.range,
            actor.attackMask,
            actor.dir);
    }
    void InitTurret()
    {
        //Initialize the firing script
        GetComponent<TurretController>().Init(
            actor,
            actor.dir,
            offense,
            ability.template.power,
            ability.template.range,
            ability.template.speed,
            ability.template.duration);
    }
    public void Init(Actor actor, BaseAbility ability, int sortingOrder)
    {
        //Cache components
        if (sortingGroup == null)
            sortingGroup = GetComponent<SortingGroup>();

        //Set the sorting order
        if (sortingGroup != null)
            sortingGroup.sortingOrder = sortingOrder;

        //Connect payload wiring
        this.actor = actor;
        this.ability = ability;

        //Calculate the damage once and cache it
        offense = 
            ability.template.damageTypes *
            (1 + ((ability.practiceLevel.valueBaseTemp - 1) * 0.25f));

        //Addition initialization for payload-specific scripts
        switch (payloadType)
        {
            case PayloadType.Projectile: InitProjectile(); break;
            case PayloadType.Turret: InitTurret(); break;
            default: break;
        }
    }
    public void UnInit()
    {
        //Cleanup
        actor = null;
        ability = null;
    }

    void OnDespawn()
    {
        UnInit();
    }
}