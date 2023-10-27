using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Inventory")]
public class Inventory : ScriptableObject, IListSource
{
    public List<BaseItem> items;

    IListRowSource[] IListSource.sourceItems
    {
        get { return items.ToArray(); }
    }

    public float weight { get; set; }
    public BaseItem equippedWeapon { get; protected set; }

    //Events
    public System.Action<BaseItem, bool> OnSingleItemChanged;
    public void NotifySingleItemChanged(BaseItem item, bool removed, bool recalc = true)
    {
        if (recalc) CalcStats();
        if (OnSingleItemChanged != null)
        {
            //Debug.Log(OnSingleItemChanged);
            OnSingleItemChanged(item, removed);
        }
    }
    public System.Action OnMultipleItemsChanged;
    public void NotifyMultipleItemsChanged(bool recalc = true)
    {
        if (recalc) CalcStats();
        if (OnMultipleItemsChanged != null)
            OnMultipleItemsChanged();
    }
    public void ClearAllEvents()
    {
        OnSingleItemChanged = null;
        OnMultipleItemsChanged = null;
    }

    #region Methods
    //Common Methods
    void IListSource.SetSource(UI_ListManager list)
    {
        OnMultipleItemsChanged += list.OnListChanged;
        OnSingleItemChanged += list.OnListChanged;
    }
    void IListSource.UnsetSource(UI_ListManager list)
    {
        OnMultipleItemsChanged -= list.OnListChanged;
        OnSingleItemChanged -= list.OnListChanged;
    }
    void IListSource.Sort()
    {
        items.Sort(new BaseItemSortByItemType());
    }
    
    /// <summary>
    /// Creates a new item object with the split amount
    /// </summary>
    /// <param name="item"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    BaseItem SplitStack(BaseItem item, int quantity)
    {
        BaseItem newStack = new BaseItem(item, quantity);
        return newStack;
    }
    /// <summary>
    /// Adds a portion of the stack of items to this inventory
    /// (Using the item's quantity value will move the entire stack)
    /// </summary>
    /// <param name="item"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public BaseItem AddItem(BaseItem item, int quantity)
    {
        bool merged = false;

        //Special handling for stackable items
        if (!item.template.isWeapon())
        {

            //Check to see if the stack needs to be split (partial move)
            if (item.quantity > quantity)
                item = SplitStack(item, quantity);

            //Check for other item stacks and add to those stacks if found
            foreach (BaseItem item2 in items)
            {
                if ((item2.template.saveID == item.template.saveID) && (item2.quality == item.quality))
                {
                    item2.quantity += item.quantity;
                    item = item2;
                    merged = true;
                    break;
                }
            }
        }

        //There no stacks of the same item - use a new item slot
        if (!merged)
            items.Add(item);

        NotifySingleItemChanged(item, false);
        return item;
    }
    /// <summary>
    /// Adds an entire stack of items to this inventory
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public BaseItem AddItem(BaseItem item)
    {
        return AddItem(item, item.quantity);
    }
    /// <summary>
    /// Adds a list of items to this inventory
    /// </summary>
    /// <param name="items"></param>
    public void AddItems(List<BaseItem> items)
    {
        foreach(BaseItem item in items)
            AddItem(item);

        //Debug.Log("Added " + items.Count + " items from loot table");
    }

    public void RemoveItem(BaseItem item)
    {
        if (items.Contains(item) && (!item.equipped))
        {
            items.Remove(item);
            NotifySingleItemChanged(item, true);
        }
    }
    public void RemoveItem(BaseItem item, int quantity)
    {
        if (quantity >= item.quantity)
            //Remove the item from the list
            RemoveItem(item);
        else
        {
            //Update the quantity
            item.quantity -= quantity;
            NotifySingleItemChanged(item, false);
        }
    }
    public void RemoveItem(ItemTemplate item, int quantity)
    {
        BaseItem theItem = FindItem(item);
        if(theItem != null)
            RemoveItem(theItem, quantity);
    }
    public string TransferTo(Inventory otherInventory)
    {
        string ret = string.Empty;

        //Transfer each item
        foreach (BaseItem item in items)
        {
            otherInventory.AddItem(item);
            ret = string.Concat(ret, 
                item.template.name + 
                " (" + item.quantity.ToString() + ")" +
                "\n");
        }

        //Empty this inventory
        items.Clear();

        NotifyMultipleItemsChanged();
        return ret;
    }
    public string TransferFrom(InventoryManifest manifest)
    {
        string ret = string.Empty;
        if (manifest != null)
        {
            BaseItem item;

            //Transfer each item
            foreach (ItemManifest itemManifest in manifest.items)
            {
                item = new BaseItem(itemManifest);

                ret = string.Concat(ret,
                    item.template.name +
                    " (" + item.quantity.ToString() + ")" +
                    "\n");

                AddItem(item);
            }

            NotifyMultipleItemsChanged();
        }

        return ret;
    }
    public BaseItem FindItem(ItemTemplate template)
    {
        BaseItem ret = null;

        foreach (BaseItem item in items)
            if (item.template == template)
            {
                ret = item;
                break;
            }

        return ret;
    }


    //Methods for equippable items
    public bool EquipItem(BaseItem item, Character character, bool putItOn = true)
    {
        if (items.Contains(item))
        {
            //Check to see if any equipment occupies the required slots and remove them
            if (putItOn)
                foreach (BaseItem itemToUnequip in character.spineEquipmentManager.getOccupiedSlotItems(item))
                    EquipItem(itemToUnequip, character, false);

            //Put on / take off item
            if (character.spineEquipmentManager.equipItem(item, putItOn))
            {
                item.equipped = putItOn;
                NotifySingleItemChanged(item, false);

                //Update visuals
                character.spineEquipmentManager.Apply();

                return true;
            }

            //Update equipped weapon if needed
            if (item.template.isWeapon())
                if (putItOn) equippedWeapon = item; else equippedWeapon = null;
        }
        else
            Debug.LogError("Tried to equip an item that is not part of our inventory");

        return false;
    }
    public void ClearAllSlots(Character character)
    {
        //character.clearEquipSlots();
        character.spineEquipmentManager.clearSlots();
    }

    //Utility Methods
    protected void CalcStats()
    {
        //Cache weight to avoid parsing long inventory lists every frame
        float __weight = 0;
        foreach (BaseItem item in items)
            __weight += item.weightTotal;

        weight = __weight;
    }
    public bool IsEmpty()
    {
        return (items.Count == 0);
    }
    public BaseItem FindAmmo(int ammoType)
    {
        foreach (BaseItem item in items)
            if (item.template.isAmmo())
                if (item.template.chargeType == ammoType)
                    return item;

        return null;
    }
    public float GetTotalValue()
    {
        float ret = 0f;
        foreach (BaseItem item in items)
            ret += item.valueTotal;

        return ret;
    }

    //Persistence Methods
    public SaveItem[] GetSaveItems()
    {
        if (items.Count > 0)
        {
            SaveItem[] ret = new SaveItem[items.Count];
            for (int i = 0; i < items.Count; i++)
                ret[i] = new SaveItem((BaseItem) items[i]);

            return ret;
        }
        else return new SaveItem[0];
    }
    public void SetSaveItems(SaveItem[] saveItems)
    {
        items.Clear();
        if(saveItems != null)
            foreach (SaveItem sItem in saveItems)
                AddItem(sItem.Unpack());

        NotifyMultipleItemsChanged(true);
    }
    #endregion
}