using UnityEngine;

[RequireComponent(typeof(PersistentObject))]
[RequireComponent(typeof(CircleCollider2D))]
public class LootableObject : MonoBehaviour 
{
    const int lootedState = 5;

    //Inspector Properties
    [Tooltip("Pre-designed items that will be found here")]
    public InventoryManifest loot;
    [Tooltip("Randomized items that can be found here")]
    public LootTable lootTable;

    //Generate items from loot table into this

    PersistentObject persistentObject;
    InteractableObject interactableObject;

    //Mono Methods
    void Awake()
    {
        persistentObject = GetComponent<PersistentObject>();
        interactableObject = GetComponent<InteractableObject>();

        interactableObject.OnInteractAccessible += Interact;

        //Already looted?
        if (persistentObject.GetState() == lootedState)
        {
            //Remove loot (no need to update the state again)
            OnLooted(false);
        }
    }

    //Main Methods
    void OnLooted(bool updateState = true)
    {
        //Disable object
        gameObject.SetActive(false);

        if (updateState)
        {
            //Update state to looted
            persistentObject.SetState(lootedState);
            //TODO: Make this compatible with interactableObject's state data
        }
    }
    bool IsLootAvailable()
    {
        return 
            loot != null && 
            loot.items != null && 
            loot.items.Count > 0;
    }
    void Interact(Actor interactor)
    {
        if (!lootTable && !IsLootAvailable())
        {
            lootTable = GameDatabase.metaGame.fallbackLootTable;
            if (!lootTable)
            {
                Debug.LogWarning(name + ": no loot assigned + fallback loot table is not assigned");
                return;
            }
        }

        if (!IsLootAvailable()) loot = lootTable.makeLoot(0);

        interactor.OnGetLoot(loot);

        OnLooted();
    }
}