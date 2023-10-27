[System.Serializable]
public class ItemRecipeEx : IListRowSource
{
    public ItemTemplate item;
    public int level = 0;
    
    int _progress = 0;
    
    //Accessors
    public int progress
    {
        get { return _progress; }
        set
        {
            if (value >= progressRequired)
            {
                level++;
                _progress = 0;
            }
            else
                _progress = value;
        }
    }
    public int progressRequired
    {
        get
        {
            return Common.smoothFunctionInt(level, 2, 6);
        }
    }

    //Methods
    public ItemRecipeEx(ItemTemplate itemTemplate)
    {
        item = itemTemplate;
    }
    public ItemRecipeEx(int itemSaveID)
    {
        item = System.Array.Find(GameDatabase.core.items, x => x.saveID == itemSaveID);
    }
}