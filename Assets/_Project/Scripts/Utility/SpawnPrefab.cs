using UnityEngine;

public class SpawnPrefab : MonoTrigger
{
    [System.Serializable]
    public struct SpawnInfo
    {
        public GameObject prefab;
        public Transform spawnPoint;
    }

    public SpawnInfo[] spawns;

    public bool usePoolManager = false;
    public bool destroyAfter = false;
    public bool disableAfter = false;

    protected GameObject[] spawnedObjects = null;

    protected override void fire()
    {
        base.fire();

        Transform spawnTrans, parentTrans;
        spawnedObjects = new GameObject[spawns.Length];

        //foreach (SpawnInfo spawnInfo in spawns)
        for(int i=0; i < spawns.Length; i++)
        {
            if (spawns[i].spawnPoint)
                spawnTrans = spawns[i].spawnPoint;
            else
                spawnTrans = transform;

            Vector3 spawnScale = spawnTrans.localScale;

            //Make sure the parent is not the object that will be popped
            if (spawnTrans == transform)
                parentTrans = spawnTrans.parent;
            else
                parentTrans = spawnTrans;

            //Common.producePrefab(spawnInfo.prefab, spawnTrans.position, spawnTrans.rotation, usePoolManager, parentTrans);
            spawnedObjects[i] = Common.ProducePrefab(spawns[i].prefab, spawnTrans, usePoolManager);

            //Reparent object to avoid destroying it when the spawner pops
            if (parentTrans != spawnedObjects[i].transform.parent)
                spawnedObjects[i].transform.SetParent(parentTrans);

            //Set scale
            spawnedObjects[i].transform.localScale = spawnScale;
        }

        //Cleanup
        if (disableAfter) gameObject.SetActive(false);
        if(destroyAfter) Destroy(gameObject);
    }
}
