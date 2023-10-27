using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using GameDB;

public class ThrowingWeapon : DepthPhysicsMotor
{
    public enum ContactAction { Nothing, Explode, HitTarget }

    #region Inspector Properties
    [Header("Actions")]
    [Tooltip("What happens when object hits an enemy")]
    public ContactAction enemyContactAction = ContactAction.HitTarget;
    [Tooltip("What happens when object hits the ground")]
    public ContactAction groundContactAction = ContactAction.Explode;
    [Tooltip("In-flight behaviour")]
    public Vector2 travelArc = new Vector2(1, 1);

    [Header("Sounds")]
    [Tooltip("Object leaves player's hand")]
    public AudioGroupTemplate throwSound;
    [Tooltip("Object hits ground")]
    public AudioGroupTemplate hitGroundSound;
    [Tooltip("Object hits enemy")]
    public AudioGroupTemplate hitTargetSound;
    [Tooltip("Object explodes")]
    public AudioGroupTemplate explodeSound;

    [Header("Effects")]
    [Tooltip("How much the screen shakes when a target is hit")]
    public float hitShakeMagnitude;
    [Tooltip("Spawns when contact action is set to HitTarget")]
    public CommonStructures.EffectSpawnRelative[] hitEffects;
    [Tooltip("How much the screen shakes when exploding")]
    public float explodeShakeMagnitude;
    [Tooltip("Spawns when contact action is set to Explode")]
    public CommonStructures.EffectSpawnRelative[] explodeEffects;

    [Header("Stat Multipliers")]
    [Tooltip("Damage modifier when contact type is set to HitTarget")]
    [Range(0,1f)]
    public float hitDamageMultiplier = 1f;
    [Tooltip("Damage modifier when contact type is set to Explode")]
    [Range(0, 1f)]
    public float explodeDamageMultiplier = 1f;
    #endregion

    [HideInInspector]
    public int direction = 1;

    //Private Properties
    SortingGroup sortingGroup = null;
    BaseItem item = null;
    LayerMask attackMask;
    HitData hitStats;
    bool ownerIsPlayer = false;

    IEnumerator DelayedInit()
    {
        //Init values
        stop();

        //Wait one frame for object to fully spawn and position itself
        yield return null;

        //Setup in-flight behaviour
        Vector3 throwArc = new Vector3(
            item.template.range * travelArc.x * direction,
            travelArc.y,
            0);

        //Start physics simulation
        startUp(throwArc, true);
    }
    public void Init(Actor owner, BaseItem item, int direction, float groundLevel)
    {
        if (sortingGroup == null)
            sortingGroup = GetComponent<SortingGroup>();

        this.attackMask = owner.attackMask;
        this.direction = direction;
        this.groundLevel = groundLevel;

        //Cache this once to avoid overheads from repeated class checking
        ownerIsPlayer = owner.controlledByPlayer;
        this.item = item;

        //Cache the stats (some values will never change)
        hitStats =
            new HitData(
                ownerIsPlayer,
                item.template.damageMatrix,
                owner == null ?  1f : owner.characterSheet.rangedWeaponCritRateModifier,
                0f, 1f, //Throwing weapons cannot crit (should they?)
                Vector3.zero, null, new DamageResult());

        //Some code needs to be done at the end of the frame
        StartCoroutine(DelayedInit());
    }
    void RegisterHit(Collider target, HitData hitinfo)
    {
        //Send the hit info to the object that was hit
        target.SendMessage(
            HitBox.msgOnAttacked,
            hitinfo, 
            SendMessageOptions.DontRequireReceiver);
    }
    void HitCheck(bool aoe)
    {
        //Check to see what was hit
        Collider[] hits = Physics.OverlapBox(
            myTrans.position,
            new Vector3(
                item.template.power,
                item.template.power,
                0.5f),
            myTrans.rotation,
            attackMask.value);

        if (hits.Length > 0)
        {
            if (aoe)
            {
                //Notify all targets that they have been hit
                foreach (Collider c in hits)
                {
                    hitStats.hitPosition = c.transform.position;
                    RegisterHit(c, hitStats);
                }
            }
            else
            {
                Collider hitToUse = hits[0];

                //Find the closest target to the me and use that one
                foreach (Collider c in hits)
                    if (Vector3.Distance(c.transform.position, myTrans.position) > 
                        Vector3.Distance(hitToUse.transform.position, myTrans.position))
                        hitToUse = c;

                //Notify target that it has been hit
                hitStats.hitPosition = hitToUse.transform.position;
                RegisterHit(hitToUse, hitStats);
            }
        }
    }
    protected virtual void HandleContact(ContactAction contactAction)
    {
        bool doCleanup = false;

        //Stop moving and spinning
        stop();

        switch (contactAction)
        {
            case ContactAction.Explode:
                //Screenshake
                if(myTrans)
                    CameraController.instance.Shake(explodeShakeMagnitude, myTrans.position);
                else
                    CameraController.instance.Shake(explodeShakeMagnitude, Vector3.zero);

                //Spawn special effects
                foreach (CommonStructures.EffectSpawnRelative es in explodeEffects)
                    Common.SpawnEffect(
                        es, 
                        myTrans, 
                        direction,
                        sortingGroup.sortingOrder + 1);

                //Check for, and apply damage to, all targets in the hitRadius range
                HitCheck(true);

                //Mark for removal
                doCleanup = true;
                break;
            case ContactAction.HitTarget:
                //Screenshake
                CameraController.instance.Shake(hitShakeMagnitude, myTrans.position);

                //Spawn special effects
                foreach (CommonStructures.EffectSpawnRelative es in hitEffects)
                    Common.SpawnEffect(
                        es, 
                        myTrans, 
                        direction,
                        sortingGroup.sortingOrder + 1);

                //Check for, and apply damage to, a single target
                HitCheck(false);

                //Mark for removal
                doCleanup = true;
                break;
        }

        if(doCleanup)
            Common.poolDespawn(myTrans);
    }
    protected override void OnCollided(CollisionTypes collisionType)
    {
        base.OnCollided(collisionType);
        switch(collisionType)
        {
            case CollisionTypes.Other:
                HandleContact(enemyContactAction);
                break;
            case CollisionTypes.Ground:
                HandleContact(groundContactAction);
                AudioManager.instance.Play(hitGroundSound, myTrans.position);
                break;
        }
    }
    void OnTriggerEnter(Collider collider)
    {
        if(LayerMaskEx.hasLayer(attackMask, collider.gameObject.layer))
          OnCollided(CollisionTypes.Other);
    }
}