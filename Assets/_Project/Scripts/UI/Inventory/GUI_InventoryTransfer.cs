using UnityEngine;
using UnityEngine.UI;

public class GUI_InventoryTransfer : MonoBehaviour
{
    public GUI_InventoryManager playerInventory, targetInventory;
    public Text leftNameLabel, rightNameLabel;

    PlayerController playerLink;

    public bool modified { get; set; }

    void UpdateDisplay()
    {
        modified = false;

        if ((playerInventory != null) && (targetInventory != null))
        {
            playerInventory.refresh = true;
            targetInventory.refresh = true;
        }
    }
    public void TransferEnded()
    {
        playerLink.InventoryTransferEnded();
        GUI_Common.instance.OnTransfer(false, null, null, string.Empty);
    }
    public void OnTransferAll()
    {
        if (targetInventory.inventoryLink.items.Count > 0)
        {
            targetInventory.inventoryLink.TransferTo(playerInventory.inventoryLink);
            //OnTransferEnded();
            modified = true;
        }
    }

    /// <summary>
    /// Connects/disconnects player and target box to this gui
    /// pass null values to deactivate
    /// </summary>
    /// <param name="targetDisplayName">The heading text that will show above the target ivnentory's side</param>
    public void Connect(PlayerController player, Inventory targetInventoryBox, string targetDisplayName)
    {
        targetInventory.inventoryLink = targetInventoryBox;
        playerLink = player;
        if (player != null)
            playerInventory.inventoryLink = player.myActor.inventory;
        else
            playerInventory.inventoryLink = null;


        //Setup names
        rightNameLabel.text = targetDisplayName;
        if (playerLink != null)
            leftNameLabel.text = playerLink.myActor.characterSheet.characterName;
        else
            leftNameLabel.text = string.Empty;

        //Internal checks
        modified = true;
    }
    public void OnItemClicked(GUI_InventoryManager inventoryManager, GUI_InventoryItemRow itemRow, SubmitState state)
    {
        GUI_Common.instance.HideItemDetails();
        BaseItem theItem = (BaseItem)itemRow.sourceLink;

        //Unequip item if applicable
        if ((theItem.equipped) && (theItem.quantity == 1))
        {
            if (playerLink.myActor.inventory == inventoryManager.inventoryLink)
                playerLink.myActor.UseItem(theItem);
        }

        //Prevent equipped items from being traded
        if ((!theItem.equipped) || (theItem.quantity > 1))
        {
            GUI_InventoryManager receiver;

            //Adding items to the left basket
            if (inventoryManager == playerInventory)
                receiver = targetInventory;
            else
                receiver = playerInventory;

            //Quantity
            int qty;
            switch (state)
            {
                case SubmitState.Negative: qty = 5; break;
                case SubmitState.Positive: qty = theItem.quantity; break;
                default: qty = 1; break;
            }

            receiver.inventoryLink.AddItem(theItem, qty);
            inventoryManager.inventoryLink.RemoveItem(theItem, qty);
        }
        else
            Debug.LogWarning("Item is still equipped or has an invalid quantity");

        modified = true;
    }

    void Update()
    {
        if (modified)
            UpdateDisplay();
    }
}