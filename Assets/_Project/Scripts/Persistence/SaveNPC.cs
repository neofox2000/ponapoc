using RPGData;

[System.Serializable]
public class SaveNPC
{
    //Unique Id for fetching data
    public int savedStateID = -1;

    //Stats/skills
    //public SaveAttribute[] stats = null;
    //public SaveSkill[] skills = null;

    //HP/MP/SP probably won't be needed since these will not change in the forseeable future

    //Inventory
    public SaveItem[] saveItems = null;

    //How many times the player has tried to haggle with this NPC
    public float haggleAttempts = 0f;
    //The multiplier for each item's base value when bartering
    public float tradingRate = 0f;

    /// <summary>
    /// Blank constructor to avoid serialization issues
    /// </summary>
    public SaveNPC()
    {

    }
    /// <summary>
    /// Copies data from "npc" into this object
    /// </summary>
    /// <param name="npc"></param>
    public SaveNPC(NPCController npc)
    {
        savedStateID = npc.persistentID;
        //stats = npc.characterSheet.GetSaveAttributes();
        //skills = npc.characterSheet.GetSaveSkills();
        saveItems = npc.myActor.inventory.GetSaveItems();

        tradingRate = npc.tradingRate;
        haggleAttempts = npc.GetRemainingHaggleAttempts();
    }

    /// <summary>
    /// Transfers data from this object into "npc"
    /// </summary>
    /// <param name="npc"></param>
    public void Unpack(NPCController npc)
    {
        //npc.characterSheet.SetSaveAttributes(stats);
        //npc.characterSheet.SetSaveSkills(skills);

        npc.myActor.inventory.SetSaveItems(saveItems);

        npc.tradingRate = tradingRate;
        npc.SetRemainingHaggleAttempts(haggleAttempts);
    }
}