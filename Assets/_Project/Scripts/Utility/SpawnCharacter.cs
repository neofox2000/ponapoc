using UnityEngine;

public class SpawnCharacter : MonoTrigger
{
    public enum SpawnCharacterType { Enemy, NPC };

    //Inspector Properties
    [SerializeField] protected GameObject entityPrefab;
    [SerializeField] protected Transform spawnPoint;

    //Accessors
    public Transform spawnedEntity { get; protected set; }
    public bool finished { get; protected set; }

    //Methods
    protected override void fire()
    {
        //Clean up the spawner
        gameObject.SetActive(false);

        //Get valid spawn point
        Transform spawnTrans = spawnPoint;
        if (!spawnTrans) spawnTrans = transform;

        //Set default variables
        int idToUse = 0;
        GameObject GO = null;

        //Spawn the character
        if (entityPrefab != null)
        {
            GO = Common.ProducePrefab(
                entityPrefab,
                spawnTrans.position,
                spawnTrans.rotation,
                false);

            GO.transform.SetParent(spawnTrans.parent);
        }
        else
            Debug.LogWarning("No prefab to spawn!");

        //Setup character properties
        if (GO != null)
        {
            spawnedEntity = GO.transform;
            spawnedEntity.SendMessage("setTemplate", idToUse);
        }

        finished = true;
    }
}