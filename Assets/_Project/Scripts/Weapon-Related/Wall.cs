using UnityEngine;
using UnityEngine.Rendering;
using GameDB;

public class Wall : MonoBehaviour
{
    [Tooltip("Time between effect spawns (prevents rapid fire effects from lagging the game)")]
    public float effectDelay = 0.1f;
    [Tooltip("Sound played when struck")]
    public AudioGroupTemplate hitSound;

    public CommonStructures.EffectSpawnRelative hitEffect;

    float hitDelayTimer = 0;
    Transform myTrans;
    SortingGroup sortingGroup = null;

    void Awake()
    {
        myTrans = transform;
        sortingGroup = GetComponent<SortingGroup>();
    }
    void Update()
    {
        if (hitDelayTimer > 0)
            hitDelayTimer -= Time.deltaTime;
    }

    void onAttacked(HitData hitInfo)
    {
        //Don't do anything if an effect was spawned recently
        if (hitDelayTimer <= 0)
        {
            //Play sound (if any)
            AudioManager.instance.Play(hitSound, myTrans);

            //Spawn effect (if any)
            if (hitEffect.prefab)
            {
                //Calculate offset
                hitEffect.offset =
                    hitInfo.hitPosition - myTrans.position;

                int sortingOrder = sortingGroup != null ? sortingGroup.sortingOrder + 1 : 1;

                //Spawn prefab at myTrans with offset and draw order.. 
                //..facing the same direction (walls never change direction)
                Common.SpawnEffect(
                    hitEffect,
                    myTrans,
                    1,
                    sortingOrder);
            }

            //Start timer (prevent spawning too many effects at once)
            hitDelayTimer = effectDelay;
        }
    }
}