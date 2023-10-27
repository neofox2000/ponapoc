using UnityEngine;
using RPGData;

[System.Serializable]
public class BaseItem : IListRowSource
{
    const int maxEquipSlotsPerCharacter = 25;

    #region Serialized Properties
    public ItemTemplate template;
    public int quantity = 1;
    public int quality = 1;
    public int chargesLeft = 0;
    public bool equipped = false;
    #endregion

    #region Accessors
    public float valueTotal
    {
        get { return template.value * quantity; }
    }
    public float power
    {
        get 
        {
            float addition = template.power * Common.smoothFunctionFloat(quality, 0, 0.75f);
            return template.power + addition; 
        }
    }
    public float weightTotal
    {
        get { return template.weight * quantity; }
    }
    #endregion

    void Construct(ItemTemplate template, int quantity = 1, int quality = 1, int chargesLeft = 0)
    {
        this.template = template;
        this.quantity = Mathf.Max(1, quantity);
        this.quality = Mathf.Max(1, quality);
        this.chargesLeft = chargesLeft;
    }
    public BaseItem(ItemTemplate template, int quantity = 1, int quality = 1, int chargesLeft = 0)
    {
        Construct(template, quantity, quantity, chargesLeft);
    }
    public BaseItem(ItemTemplate template, int quantity = 1, int quality = 1, float chargesPercent = 0f)
    {
        Construct(
            template, 
            quantity, 
            quality,
            Mathf.RoundToInt(template.chargeCount * chargesPercent));
    }
    public BaseItem(BaseItem existingItem, int quantity)
    {
        Construct(
            existingItem.template, 
            quantity, 
            existingItem.quality, 
            existingItem.chargesLeft);
    }
    public BaseItem(ItemManifest item)
    {
        Construct(
            item.itemTemplate, 
            item.quantity, 
            item.quality, 
            item.GetChargeCount());
    }
    /// <summary>
    /// For creating an item from a saved state
    /// </summary>
    /// <param name="saveId"></param>
    /// <param name="quantity"></param>
    /// <param name="quality"></param>
    /// <param name="chargesLeft"></param>
    /// <param name="equipped"></param>
    public BaseItem(int saveId, int quantity, int quality, int chargesLeft, bool equipped)
    {
        //Find template in database
        ItemTemplate itemTemplate = System.Array.Find(GameDatabase.sItems, x => x.saveID == saveId);

        //Do the actual constructor
        Construct(
            itemTemplate,
            quantity,
            quality,
            chargesLeft);

        //Set equipped to previously saved state
        this.equipped = equipped;
    }
}

public class BaseItemSortByItemType : System.Collections.Generic.Comparer<BaseItem>
{
    public override int Compare(BaseItem x, BaseItem y)
    {
        if (x.template.itemType != y.template.itemType)
        {
            if (x.template.itemType == ItemTypes.Throwable)
                return -1;
            else 
                if(y.template.itemType == ItemTypes.Throwable)
                    return 1;
                else
                    return x.template.itemType.CompareTo(y.template.itemType);
        }
        else
            if (x.template.saveID != y.template.saveID)
            return x.template.saveID.CompareTo(y.template.saveID);
        else
                if (x.quality != y.quality)
            return x.quality.CompareTo(y.quality);
        else
                    if (x.quantity != y.quantity)
            return x.quantity.CompareTo(y.quantity);
        else
                        if (x.chargesLeft != y.chargesLeft)
            return x.chargesLeft.CompareTo(y.chargesLeft);
        else
            return x.equipped.CompareTo(y.equipped); 
    }
}