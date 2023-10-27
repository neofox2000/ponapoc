using UnityEngine;
using System;

public class GUI_InventoryQuickSlotsManager : MonoBehaviour
{
    public GUI_InventoryManager inventory;
    public GUI_InventoryQuickSlot[] weaponSlots;
    public GUI_InventoryQuickSlot[] throwableSlots;
    public GUI_InventoryQuickSlot[] consumableSlots;

    GUI_InventoryQuickSlot[] slots;
    GUI_InventoryQuickSlot slotInFocus = null;
    bool assignmentMode = false;

    //Subscriber(s): GUI_CharacterSheet
    public Action<GUI_InventoryQuickSlot> OnQuickSlotSelected;

    void Awake()
    {
        //Cache all slots into a single array for easy usage
        slots = new GUI_InventoryQuickSlot[
            weaponSlots.Length +
            throwableSlots.Length +
            consumableSlots.Length];

        int c = 0;
        for (int i = c; i < c + weaponSlots.Length; i++)
        {
            //Cache slot
            slots[i] = weaponSlots[i - c];
            //Set slot index
            slots[i].slotIndex = i - c;
        }
        c += weaponSlots.Length;
        for (int i = c; i < c + throwableSlots.Length; i++)
        {
            //Cache slot
            slots[i] = throwableSlots[i - c];
            //Set slot index
            slots[i].slotIndex = i - c;
        }
        c += throwableSlots.Length;
        for (int i = c; i < c + consumableSlots.Length; i++)
        {
            //Cache slot
            slots[i] = consumableSlots[i - c];
            //Set slot index
            slots[i].slotIndex = i - c;
        }

        //Subscribe to slot events
        foreach (GUI_InventoryQuickSlot slot in slots)
        {
            slot.OnUsed += UseSlot;
            slot.OnClear += ClearSlot;
            slot.OnSelected += QuickSlotSelected;
        }

        //Subscript to inventory events
        inventory.OnQuickSlotItemPicked += SlotItemPicked;
    }

    public void LoadFromData()
    {
        //Load weapon slot data from PlayerData
        for (int i = 0; i < weaponSlots.Length; i++)
            weaponSlots[i].Set(
                inventory.inventoryLink.FindItem(
                    GameDatabase.sPlayerData.quickslotData.GetWeaponSlot(i)));
        //Load throwable slot data from PlayerData
        for (int i = 0; i < throwableSlots.Length; i++)
            throwableSlots[i].Set(
                inventory.inventoryLink.FindItem(
                    GameDatabase.sPlayerData.quickslotData.GetThrowableSlot(i)));
        //Load consumable slot data from PlayerData
        for (int i = 0; i < consumableSlots.Length; i++)
            consumableSlots[i].Set(
                inventory.inventoryLink.FindItem(
                    GameDatabase.sPlayerData.quickslotData.GetConsumableSlot(i)));
    }
    public void ClearAll()
    {
        if(slots != null)
            foreach (GUI_InventoryQuickSlot slot in slots)
                slot.Set(null);
    }
    void SelectFocusedSlot()
    {
        if (slotInFocus != null)
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(slotInFocus.gameObject);
    }
    void ToggleAllInteractable(bool interactable)
    {
        //In testing mode slots will be null at first
        if (slots == null) return;

        foreach (GUI_InventoryQuickSlot slot in slots)
            slot.interactable = interactable;
    }
    public void CancelSlotAssignmentProcess(bool reselectFocused)
    {
        assignmentMode = false;

        //Revert to normal
        inventory.SetStandardSelectionMode();

        //Disable interactable states for all slots
        ToggleAllInteractable(true);

        //Re-select focused slot if available
        if (reselectFocused)
            SelectFocusedSlot();
    }
    void UseSlot(GUI_InventoryQuickSlot slot)
    {
        //Try to set Inventory to 'Quick-Slot assignment mode', filtering by Weapon
        if (inventory.SetQuickSlotPickMode(slot.slotType))
        {
            assignmentMode = true;

            //Cache slot for callback use
            //slotInFocus = slot;

            //Debug.Log("Slot focus = " + slot);

            //Disable interactable states for all slots
            ToggleAllInteractable(false);
        }
        else
            Alerter.ShowMessage("You don't have any " + slot.slotType + "'s");
    }
    void ClearSlot(GUI_InventoryQuickSlot slot)
    {
        //Clear visual slot
        slot.Set(null);

        //Store change
        GameDatabase.sPlayerData.quickslotData.SetItemQuickSlot(
            slot.slotType,
            slot.slotIndex,
            null);
    }
    void QuickSlotSelected(GUI_InventoryQuickSlot slot)
    {
        if (!assignmentMode)
        {
            slotInFocus = slot;

            //Fire event
            if (OnQuickSlotSelected != null)
                OnQuickSlotSelected(slot);
        }
    }
    void SlotItemPicked(BaseItem item)
    {
        //Make sure something didn't go horribly wrong
        if (slotInFocus != null)
        {
            assignmentMode = false;

            if (slotInFocus.slotType == item.template.itemType)
            {
                //Assign item to slot
                slotInFocus.Set(item);

                //Store change
                GameDatabase.sPlayerData.quickslotData.SetItemQuickSlot(
                    slotInFocus.slotType,
                    slotInFocus.slotIndex,
                    item.template);
            }

            //Re-enable interactable states for all slots
            ToggleAllInteractable(true);

            //Re-select focused slot if available
            SelectFocusedSlot();
        }
        else
            Debug.LogError("Trying to assign quickslot item without having first chosen a slot!");
    }
}