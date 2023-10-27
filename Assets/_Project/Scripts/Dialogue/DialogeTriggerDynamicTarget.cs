using UnityEngine;
using System.Collections;
using Variables;

public class DialogeTriggerDynamicTarget : MonoBehaviour
{
    [System.Serializable]
    public class DynamicTargetGroup
    {
        public bool player = false;
        public SpawnCharacter spawner;

        [HideInInspector]
        public Transform actor;
    }

    public bool fireOnce = true;
    public DynamicTargetGroup[] dtgActors;

    void Awake()
    {
        //StartCoroutine(assignSpawnerThings());
    }

    bool didStuffSpawnYet()
    {
        foreach (DynamicTargetGroup dtg in dtgActors)
        {
            //Player type
            if (dtg.player)
            {
                if (GameDatabase.localPlayer == null)
                    return false;
            }
            else
            {
                if (!dtg.spawner.finished)
                    return false;
                //if (!dtg.spawner || (dtg.spawner.spawnedCharacter == null))
                    //return false;
            }
        }

        return true;
    }
    void setActor(DynamicTargetGroup dtg)
    {
        if (dtg.player)
            dtg.actor = GameDatabase.localPlayer.transform;
        else
            dtg.actor = dtg.spawner.spawnedEntity;
    }
    IEnumerator assignSpawnerThings()
    {
        while(!didStuffSpawnYet())
            yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(0.1f);

        yield return new WaitForEndOfFrame();
        foreach (DynamicTargetGroup dtg in dtgActors)
            setActor(dtg);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        foreach (DynamicTargetGroup dtg in dtgActors)
            setActor(dtg);

        if (fireOnce) gameObject.SetActive(false);
        //NPCController controller = dtgActors[1].actor.GetComponentInChildren<NPCController>();
        //controller.interact(dtgActors[0].actor);
        dtgActors[1].actor.SendMessage("interact", dtgActors[0].actor);
    }
}
