using UnityEngine;

[CreateAssetMenu(menuName = "Data/Loot Table")]
public class LootTable : ScriptableObject
{
    [Tooltip("Number of times to roll the dice for items")]
    [Range(1, 100)]
    public int rolls;
    public LootTableEntry[] loot;

    [System.NonSerialized] bool sorted = false;

    public InventoryManifest makeLoot(int extraRolls)
    {
        InventoryManifest lootMade = new InventoryManifest();

        LootTableEntry chosenEntry =
            new LootTableEntry(null, 0, 1, 1);

        for (int i = 0; i < (rolls + extraRolls); i++)
        {
            //Sort by chance ascending to give the least likely loot a proper chance to spawn
            if (!sorted)
            {
                sorted = true;
                System.Array.Sort(loot, new LootTableEntrySortByChance());
            }

            foreach (LootTableEntry lte in loot)
            {
                float roll = Random.Range(0f, 1f);
                if (lte.chance >= roll)
                {
                    chosenEntry = lte;
                    break;
                }
            }

            //Roll successful
            //if ((chosenItemType.itemID >= 0) && (chosenItemType.itemID < GameDatabase.dbs.items.recs.Count))
            if (chosenEntry.item != null)
            {
                int quantity;

                //Don't drop multiple equippables or consumables
                if ((chosenEntry.item.isWeapon()) || (chosenEntry.item.isApparel()) ||
                        //If designers forgot to set quantities, then make 1 by default
                        (chosenEntry.minQuantity == 0) || (chosenEntry.maxQuantity == 0))
                    quantity = 1;
                else
                {
                    quantity = Random.Range(
                        chosenEntry.minQuantity,
                        chosenEntry.maxQuantity);
                }

                //Add loot to list
                lootMade.AddItem(chosenEntry.item, quantity);
            }
        }

        return lootMade;
    }
}