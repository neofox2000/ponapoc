using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LootTableEntry
{
    public ItemTemplate item;
    [Range(0, 1f)]
    public float chance;
    [Range(1, 999)]
    public int minQuantity;
    [Range(1, 999)]
    public int maxQuantity;

    public LootTableEntry(ItemTemplate item, float chance, int minQuantity, int maxQuantity)
    {
        this.item = item;
        this.chance = chance;
        this.minQuantity = minQuantity;
        this.maxQuantity = maxQuantity;
    }
}
public class LootTableEntrySortByChance : Comparer<LootTableEntry>
{
    public override int Compare(LootTableEntry x, LootTableEntry y)
    {
        if (x.chance != y.chance)
            return x.chance.CompareTo(y.chance);
        else
            return x.item.saveID.CompareTo(y.item.saveID);
    }
}