using System;
using UnityEngine.UI;
using RPGData;
using Variables;

public class GUI_InventoryManager : UI_ListManager 
{
    [Serializable]
    public enum InventoryModes { Standard, Barter, Crafting, Transfer, QuickSlotPick };

    public InventoryModes inventoryMode = InventoryModes.Standard;
    public Text weight;

    public Action<BaseItem> OnQuickSlotItemPicked;

    public Inventory inventoryLink
    {
        get { return (Inventory)sourceLink;  }
        set { sourceLink = value; }
    }

    public Action<UI_ListItem> OnItemSelected;

    protected override void RowSubmit(UI_ListItem itemRow, SubmitState state)
    {
        base.RowSubmit(itemRow, state);

        switch(inventoryMode)
        {
            case InventoryModes.Standard:
                ((PlayerController)GameDatabase.localPlayer).ClickItem(
                    (BaseItem)itemRow.sourceLink, state);
                break;
            case InventoryModes.Transfer:
                GUI_Common.instance.guiInventoryTransfer
                    .OnItemClicked(this, (GUI_InventoryItemRow)itemRow, state);
                break;
            case InventoryModes.Barter:
                GUI_Common.instance.guiBarter
                    .ItemClicked(this, (GUI_InventoryItemRow)itemRow, state);
                break;
            case InventoryModes.Crafting:
                GUI_Common.instance.guiCrafting
                    .OnItemClicked((GUI_InventoryItemRow)itemRow, state);
                break;
            case InventoryModes.QuickSlotPick:
                //Fire event
                if (OnQuickSlotItemPicked != null)
                    OnQuickSlotItemPicked((BaseItem)((GUI_InventoryItemRow)itemRow).sourceLink);

                //Go back to standard mode?
                inventoryMode = InventoryModes.Standard;
                RemoveFilter();
                break;
        }
    }
    protected override void RowCancel(UI_ListItem itemRow)
    {
        base.RowCancel(itemRow);

        switch (inventoryMode)
        {
            //TODO: Add prompt?
            case InventoryModes.Standard:
            case InventoryModes.Transfer:
                //inventoryLink.RemoveItem((BaseItem)((GUI_InventoryItemRow)itemRow).sourceLink, 1);
                RowSubmit(itemRow, SubmitState.Negative);
                break;
            case InventoryModes.QuickSlotPick:
                //Cancel pick mode
                break;
        }
    }
    protected override void RowSelect(UI_ListItem row)
    {
        base.RowSelect(row);

        //Fire event
        if (OnItemSelected != null)
            OnItemSelected(row);
    }
    protected override void SetupDisplay()
    {
        base.SetupDisplay();

        if (sourceLink != null)
            if(weight)
                weight.text = ((Inventory) sourceLink).weight.ToString("#,0.0");
    }
    public bool SetQuickSlotPickMode(ItemTypes slotType)
    {
        //Try to filter by slot type
        bool ret = FilterByItemType(slotType);

        if (ret)
        {
            //Set mode
            inventoryMode = InventoryModes.QuickSlotPick;

            //Select first available item
            SelectFirstAvailable();
        }
        else
            RemoveFilter();

        return ret;
    }
    public void SetStandardSelectionMode()
    {
        //Set mode
        inventoryMode = InventoryModes.Standard;

        //Remove any existing filter
        RemoveFilter();
    }

    //Filtering
    /// <summary>
    /// Toggles item interactability based on type filter
    /// </summary>
    /// <param name="itemType">The type of items that should be interactive</param>
    /// <returns>True if inventory contains the type of items requested</returns>
    bool FilterByItemType(ItemTypes itemType)
    {
        bool foundItem = false;

        //Set all rows to interactable or not based on item type
        foreach (GUI_InventoryItemRow row in rows)
        {
            //Make sure the row is valid (removed items disable rows rather than delete)
            if (row.gameObject.activeSelf)
                //Set the row interaction based on the required filter
                if(row.ApplyFilter(itemType))
                    //Flag a found item for function return
                    foundItem = true;
        }

        return foundItem;
    }
    void RemoveFilter()
    {
        //In testing mode, rows will not yet be initialized when this function is called
        if (rows == null) return;

        //Set all rows to interactable
        foreach (GUI_InventoryItemRow row in rows)
            //Make sure row is valid
            if (row.gameObject.activeSelf)
                row.RemoveFilter();
    }
    void SelectFirstAvailable()
    {
        foreach (GUI_InventoryItemRow row in rows)
            //Make sure row is valid
            if (row.gameObject.activeSelf)
                if (row.selectable.interactable)
                {
                    UnityEngine.EventSystems.EventSystem.current
                        .SetSelectedGameObject(row.gameObject);
                    return;
                }
    }
}
