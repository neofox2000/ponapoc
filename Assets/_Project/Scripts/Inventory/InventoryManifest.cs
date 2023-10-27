using System.Collections.Generic;

[System.Serializable]
public class InventoryManifest
{
    public List<ItemManifest> items = new List<ItemManifest>();

    public void AddItem(ItemTemplate item, int quantity, int quality = 1, float durability = 1f, float charge = 1f)
    {
        items.Add(new ItemManifest(item, quantity, quality, charge, 1f, durability));
    }
}