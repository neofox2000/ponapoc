[System.Serializable]
public class SaveIRE
{
    public int ID;
    public int level, progress;

    public SaveIRE()
    {
        ID = 0;
        level = 0;
        progress = 1;
    }
    public SaveIRE(ItemRecipeEx ire)
    {
        ID = ire.item.saveID;
        level = ire.level;
        progress = ire.progress;
    }
    public ItemRecipeEx unpack()
    {
        ItemRecipeEx ire = new ItemRecipeEx(ID);
        ire.level = level;
        ire.progress = progress;

        return ire;
    }
}