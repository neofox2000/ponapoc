[System.Serializable]
public class SaveItem
{
    //Item template ID
    public int ID;
    public int quantity, quality, chargesLeft;
    public bool equipped;

    public SaveItem()
    {
        ID = 0;
        quantity = 1;
        
        chargesLeft = 0;
        equipped = false;
    }
    public SaveItem(BaseItem item)
    {
        ID = item.template.saveID;
        quantity = item.quantity;
        quality = item.quality;
        chargesLeft = item.chargesLeft;
        equipped = item.equipped;
    }
    public BaseItem Unpack()
    {
        return new BaseItem(
            ID, 
            quantity, 
            quality,
            chargesLeft, 
            equipped);
    }
}
