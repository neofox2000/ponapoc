using UnityEngine;

namespace RPGData
{
    [System.Serializable]
    public class RecipeRequirement
    {
        public ItemTemplate item;
        [Range(0, 64000)]
        public int quantity;
        [Range(0f, 1f)]
        public float chance;

        public RecipeRequirement(ItemTemplate item, int quantity, float chance)
        {
            this.item = item;
            this.quantity = quantity;
            this.chance = chance;
        }
    }
}