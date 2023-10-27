using System;
using UnityEngine;

[Serializable]
public struct ItemManifest
{
    public ItemTemplate itemTemplate;
    [Range(1, 64000)]
    public int quantity;
    [Range(1, 99)]
    public int quality;
    [Range(0f, 1f)]
    public float charge;
    [Tooltip("If this item has to be rolled for, how likely is it to appear?")]
    [Range(0f, 1f)]
    public float chance;
    [Range(0f, 1f)]
    public float durability;

    public ItemManifest(ItemTemplate itemTemplate, int quantity, int quality, float charge, float durability, float chance)
    {
        this.itemTemplate = itemTemplate;
        this.quantity = quantity;
        this.quality = quality;
        this.charge = charge;
        this.durability = durability;
        this.chance = chance;
    }
    public int GetChargeCount()
    {
        return Mathf.RoundToInt(itemTemplate.chargeCount * charge);
    }
}