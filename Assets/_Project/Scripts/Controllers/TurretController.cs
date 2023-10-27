using UnityEngine;
using System.Linq;
using GameDB;

/// <summary>
/// Autonomous gun platform.
/// Fires on connected controller's enemies.
/// Uses connected controller's ability level.
/// </summary>
public class TurretController : DeployableObjectController
{
    #region Inspector Properties
    [Header("Turret Settings")]
    public Transform muzzle;
    public Transform ejectionPoint;
    public float thinkTime = 0.25f;
    public string fireAnimKey = "Fire";
    public AudioGroupTemplate fireSound;
    public CommonStructures.EffectSpawnRelative fireEffect;
    public CommonStructures.EffectSpawnRelative tracerEffect;
    public CommonStructures.EffectSpawnRelative ejectionEffect;
    #endregion

    #region Private Properties
    bool turretInitialized = false;
    float range = 15f, rateOfFire = 0.75f;
    float thinkTimer = 0, fireTimer = 0;
    Transform currentTarget = null;
    Actor owner = null;
    #endregion

    #region Accessors
    protected override bool online
    {
        get { return base.online; }
        set
        {
            base.online = value;
            OnlineStateChanged();
        }
    }
    #endregion

    #region Startup/Shutdown Routines
    public void Init(Actor owner, int startingDirection, ForceMatrix damage, float critRate, float range, float rateOfFire, float duration)
    {
        this.owner = owner;
        //How do?
        //base.damage = damage;
        //this.critRate = critRate;
        this.range = range;
        this.rateOfFire = rateOfFire;
        myActor.attackMask = owner.attackMask;

        turretInitialized = true;

        //Call base class Init function
        Init(owner, startingDirection, duration);
    }
    void OnlineStateChanged()
    {
        //Clear target
        currentTarget = null;

        if (online)
        {
            //Reset think timer
            thinkTimer = thinkTime;

            //After deploy (hopefully) targetter bone should be in the correct default position
            //defaultTargetPosition = target.localPosition;

        }
    }
    #endregion

    #region Weapon Routines
    void SendHitMessage(Collider receiver, Vector3 hitPoint, int hitCount)
    {
        //Send the hit info to the object that was hit
        HitData hitinfo = new HitData(
            owner.controlledByPlayer,
            myActor.characterSheet.damage, 
            1f,
            myActor.characterSheet.critRate,
            1.5f, 
            hitPoint, 
            null,
            new DamageResult());

        //Send hit message to object
        receiver.SendMessage(
            HitBox.msgOnAttacked, 
            hitinfo, 
            SendMessageOptions.DontRequireReceiver);
    }
    void RegisterHits(RaycastHit[] rangedHits, Collider[] pointBlankHits, Vector3 emitterPosition)
    {
        //How many objects the piercing shot has already gone through
        int hitCount = 0;

        //Process pointBlankHits first in case piercing factor is not enough to get through all the enemies
        if ((pointBlankHits != null) && (pointBlankHits.Length > 0))
        {
            //Order pointBlankHits by distance
            pointBlankHits = pointBlankHits.OrderBy(h => h.transform.position.x - emitterPosition.x).ToArray();
            foreach (Collider hit in pointBlankHits)
            {
                //Don't hit self
                if (!myActor.IsHitBoxMineOrTeams(hit))
                {
                    SendHitMessage(hit, emitterPosition, hitCount);
                    hitCount++;
                }
            }
        }

        //Process rangedHits next
        if ((rangedHits != null) && (rangedHits.Length > 0))
        {
            //Order rangedHits by distance
            rangedHits = rangedHits.OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in rangedHits)
            {
                //Don't hit self
                if (!myActor.IsHitBoxMineOrTeams(hit.collider))
                {
                    SendHitMessage(hit.collider, hit.point, hitCount);
                    hitCount++;
                }
            }
        }

    }
    float CalcHitDistance(float piercingFactor, float maxRange, Vector3 emitterPosition, Collider[] pointBlankHits, RaycastHit[] rangedHits)
    {
        //Shot travels full weapon range distance by piercing all objects it hits
        //nb: does not neccessarily damage all of them
        if (piercingFactor > 0)
            return maxRange;

        //Shot stopped by the object it hit at the muzzle
        if ((pointBlankHits != null) && (pointBlankHits.Length > 0))
            return 0;

        //Shot stopped by the object it hit at a distance
        if ((rangedHits != null) && (rangedHits.Length > 0))
            return Vector3.Distance(emitterPosition, rangedHits[0].point);

        return maxRange;
    }
    void DoTracerEffects(Transform anchor, Vector3 start, Vector3 end)
    {
        //Spawn tracer prefab
        TracerEffect tEffect = Common.SpawnEffect(
            tracerEffect, 
            anchor,
            myActor.dir,
            0)
            .GetComponent<TracerEffect>();

        //Setup tracer points and draw order
        tEffect.fire(start, end, null);
    }
    /// <summary>
    /// Called from Spine animation
    /// </summary>
    public void attackRangedHitCheck()
    {
        RaycastHit singleHit;
        RaycastHit[] rangedHits = null;
        Collider[] pointBlankHits = null;
        Vector3 shotDirection, emitterPosition;

        //Play firing sound (temporary code placement - use animation event!)
        AudioManager.instance.Play(fireSound, myTrans);

        //Throw hit checks
        emitterPosition = muzzle.position;

        //Render fire effect (muzzle flash, smoke, etc)
        Common.SpawnEffect(
            fireEffect, 
            muzzle, 
            myActor.dir,
            //sortingGroup.sortingOrder + 1);
            1);

        //Render ejection effects (bullet casings, etc)
        BulletCasingPhysics casing =
            Common.SpawnEffect(
                ejectionEffect,
                ejectionPoint,
                myActor.dir,
                //sortingGroup.sortingOrder + 1)
                1)
            .GetComponent<BulletCasingPhysics>();

        if (casing) casing.doInit(myActor.dir, myTrans.position.y);

        //Calculate direction for hit check
        shotDirection = Vector3.right * myActor.dir;

        //Set point-blank hit distance for tracers
        float hitDist = 0.1f;

        //Get point-blank hits (rays that start inside a collider do not detect that collider)
        pointBlankHits = Physics.OverlapBox(
            emitterPosition,
            Vector3.one * 0.01f,
            Quaternion.identity,
            myActor.attackMask.value);

        //Get ranged hit
        if (Physics.Raycast(emitterPosition, shotDirection, out singleHit, range, myActor.attackMask.value))
            if (singleHit.collider)
                rangedHits = new RaycastHit[1] { singleHit };

        //Send hit notifications to the target(s)
        RegisterHits(
            rangedHits,
            pointBlankHits,
            emitterPosition);

        //Set distance for tracer
        hitDist = CalcHitDistance(
            0,
            range,
            emitterPosition,
            pointBlankHits,
            rangedHits);

        //Draw tracer effect(s)
        DoTracerEffects(
            muzzle,
            emitterPosition,
            emitterPosition + (shotDirection * hitDist));
    }

    void Aim()
    {
        //target.position = currentTarget.position + new Vector3(0, -1, 0);
    }
    void Fire()
    {
        fireTimer = rateOfFire;
        //animator.SetTrigger(fireAnimKey);
    }
    #endregion

    //AI Routines
    //Find new target
    bool DetectTargets()
    {
        RaycastHit hit;
        if (Physics.Raycast(muzzle.position, Vector3.right * myActor.dir, out hit, range, myActor.attackMask))
        {
            //AIController target = hit.collider.GetComponentInParent<AIController>();
            currentTarget = hit.transform;
            return true;
        }

        return false;
    }
    void UnTarget()
    {
        currentTarget = null;
    }
    void Act()
    {
        //Reset timer
        thinkTimer = thinkTime;

        //Do AI routines
        if (fireTimer <= 0)
        {
            if (DetectTargets())
                Fire();
            else
                UnTarget();
        }

    }

    //Mono
    protected override void Update()
    {
        base.Update();

        if(turretInitialized && online)
        {
            if(fireTimer > 0) fireTimer -= Time.deltaTime;
            if (thinkTimer > 0) thinkTimer -= Time.deltaTime; else Act();
            if(currentTarget) Aim();
        }
    }
}