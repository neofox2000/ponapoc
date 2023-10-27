using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class LootPickup : MonoBehaviour
{
    public string displayName = "Loot";

    [HideInInspector]
    public Inventory inventory = null;

    //The loot is destroyed on the next frame, not instantly.  
    //This flag prevents one of the other 3 hitboxes from trying...
    //... to grab loot from a bag that has already been looted
    bool grabbed;

    public InteractableObject interactableObject { get; private set; }

    void OnSpawn()
    {
        grabbed = false;

        if (!interactableObject)
        {
            interactableObject = GetComponent<InteractableObject>();
            interactableObject.OnInteractAccessible += OnInteract;
        }
    }
    void OnInteract(Actor interactor)
    {
        if (!grabbed)
        {
            //Transfer items to interactor
            grabbed = true;
            interactor.SendMessage("OnGetLoot", inventory);

            //Despawn Bag if empty
            OnLooted();
        }
    }
    public void OnLooted()
    {
        if (inventory.IsEmpty())
        {
            //Despawn Bag
            Common.poolDespawn(transform);
        }
    }
    public void Merge(Inventory itemsToAdd)
    {
        itemsToAdd.TransferTo(inventory);
    }
}
