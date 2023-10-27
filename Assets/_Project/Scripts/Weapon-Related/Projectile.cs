using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Projectile : MonoBehaviour 
{
    public bool aoe = true;
    public float aoeRadius = 1f;
    public float speed = 15f;
    public float distance = 10f;
    public CommonStructures.EffectSpawnRelative[] explodeEffects;

    Actor owner = null;
    LayerMask collisionMask;
    Transform myTrans = null;
    SortingGroup sortingGroup = null;
    Collider _collider = null;

    int dir = 1;
    bool exploded = false;
    ForceMatrix damage;
    float critChance = 0;
    Vector3 startPosition;

    public void Init(Actor owner, ForceMatrix damage, float critChance, float speed, float distance, LayerMask collisionMask, int dir)
    {
        //Cache components
        if(myTrans == null) myTrans = transform;
        if (sortingGroup == null) sortingGroup = GetComponent<SortingGroup>();
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        //Setup properties
        exploded = false;
        this.owner = owner;
        this.collisionMask = collisionMask;
        this.dir = dir;
        this.damage = damage;
        this.critChance = critChance;
        this.speed = speed;
        this.distance = distance;

        startPosition = myTrans.position;
    }
    void SendHitBoxHit(Collider collider, HitData hitInfo)
    {
        collider.SendMessage(
            HitBox.msgOnAttacked, 
            hitInfo, 
            SendMessageOptions.DontRequireReceiver);
    }
    void Explode(Collider collider)
    {
        exploded = true;

        //Spawn explosion effects
        foreach (CommonStructures.EffectSpawnRelative ES in explodeEffects)
            Common.SpawnEffect(
                ES, 
                myTrans, 
                dir, 
                sortingGroup.sortingOrder + 1);

        if ((collider) && (collider != null))
        {
            //Create the hit info that will be sent to the objects that were hit
            HitData hitinfo = new HitData(
                owner.controlledByPlayer,
                hitinfo.damage = damage,
                1f,
                critChance,
                2f,
                Vector3.zero,
                null,
                new DamageResult());

            if (aoe)
            {
                RaycastHit[] hits;
                hits = Physics.SphereCastAll(
                    new Vector3(
                        myTrans.position.x,
                        myTrans.position.y,
                        myTrans.position.z - 5f),
                    aoeRadius,
                    Vector3.forward,
                    10f,
                    collisionMask);

                foreach (RaycastHit hit in hits)
                {
                    hitinfo.hitPosition = hit.point;
                    SendHitBoxHit(hit.collider, hitinfo);
                }
            }
            else
            {
                hitinfo.hitPosition = myTrans.position;
                SendHitBoxHit(collider, hitinfo);
            }
        }

        Common.poolDespawn(myTrans);
    }
    void Move()
    {
        //Handle projectile movement
        myTrans.position = new Vector3(
            myTrans.position.x + (dir * speed * Time.deltaTime),
            myTrans.position.y,
            myTrans.position.z);
    }
    bool DistanceCovered()
    {
        return Vector3.Distance(myTrans.position, startPosition) >= distance;
    }

	void Update () 
    {
        if (!exploded)
        {
            if (!DistanceCovered())
                Move();
            else
                Explode(null);
        }
	}
    void OnTriggerEnter(Collider collider)
    {
        //Don't explode more than once
        //Don't hit owner
        if ((!exploded) && (!owner.IsHitBoxMineOrTeams(collider)))
        {
            if ((collisionMask.value & 1 << collider.gameObject.layer) > 0)
                Explode(collider);
        }
    }
}