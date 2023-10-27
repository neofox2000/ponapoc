using UnityEngine;
using UnityEngine.Serialization;
using GameDB;

[RequireComponent(typeof(PersistentObject))]
[RequireComponent(typeof(InteractableObject))]
public class StorageController : MonoBehaviour
{
    [FormerlySerializedAs("containerDisplayName")]
    [Tooltip("The name that the player see at the top of the storage window")]
    public string displayName = "Container";
    [Tooltip("Items that will be found here the first time it is opened")]
    public InventoryManifest initialLoot;

    Inventory inventory;
    InteractableObject interactableObject;
    PersistentObject persistentObject;

    void Awake ()
    {
        inventory = new Inventory();
        persistentObject = GetComponent<PersistentObject>();
        interactableObject = GetComponent<InteractableObject>();
        interactableObject.OnInteractAccessible += interact;
    }
    void interact(Actor interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player)
        {
            //Create mission data entry if one does not exist
            LocationState missionData = GameDatabase.sPlayerData.GetCurrentLocationState();
            LocationState.ObjectStorage storageObjectData = missionData.objectStorageList.Find(x => x.id == persistentObject.id);
            if (storageObjectData != null)
            {
                //Link existing entry
                inventory = storageObjectData.inventory;
            }
            else
            {
                //Add initial loot to inventory
                inventory.TransferFrom(initialLoot);

                //Create new entry
                missionData.objectStorageList.Add(
                    new LocationState.ObjectStorage(
                        persistentObject.id, 
                        inventory));
            }

            //Open inventory swap screen
            GUI_Common.instance.OnTransfer(true, player, inventory, displayName);
        }
        else
            Debug.LogWarning("Non-Player entity is attempting to interact with me!");
    }
}