using UnityEngine;
using RPGData;

[CreateAssetMenu(menuName = "Templates/Item")]
public class ItemTemplate : ScriptableObject, IStatusEffectSource
{
    public enum WeaponClasses { None, Melee_1h, Melee_2h, Ranged_1h, Ranged_2h }

    public int saveID;
    //[Tooltip("Used for sorting items in the inventory GUI (Ascending)")]
    //public int sortingOrder;
    [TextArea(2, 10)]
    public string description;

    [Header("UI")]
    public Sprite icon;
    public Color32 iconColor;
    [Tooltip("Sound made when equipping item")]
    public AudioGroupTemplate equipSound;

    [Header("Stats")]
    public ItemTypes itemType;
    [Tooltip("Weapons Only: This template dictates the visual and audible aspects of a weapon when used within missions.")]
    public WeaponTemplate weaponTemplate;
    [Tooltip("Weapons: The types and ratios of damage dealt.\nApparel: The types and ratios of damage protection provided.")]
    public ForceMatrix damageMatrix;
    [Tooltip("Prefab to use when certain types of items need to be represented in game space.")]
    public GameObject worldPrefab;
    [Tooltip("Which ability this book item will improve.")]
    public AbilityTemplate ability;
    [Tooltip("Ammo-typed items with this number will be considered valid charges for Weapon-type items with the same number.")]
    public int chargeType;
    [Tooltip("The number of charges this item can hold.")]
    public int chargeCount;
    [Tooltip("The number of charges used per activation.")]
    public int chargeUse;
    [Tooltip("The number of activations to repeat when this weapon is activated.")]
    public int shotCount;
    [Tooltip("How effective this item is.\nThrowables: AoE Radius (0 = no aoe damage)\nConsumables: Effect time")]
    public float power;
    [Tooltip("How heavy this item is.")]
    public float weight;
    [Tooltip("Currently, this is only used for weapon fire rates.\nTODO: implement item-use cooldown.")]
    public float useDelay;
    [Tooltip("The length of time it takes to completely reload a clip-based weapon, or the time it takes for each shell to load on a shell-based weapon.")]
    public float reloadDelay;
    [Tooltip("How far away hit-checks are made when using a ranged weapon.\nHow large a hitbox to draw when checking hits for a melee weapon.\nHow far a throwing weapon will fly.")]
    public float range;
    [Tooltip("Determines how valuable an NPC considers this item when bartering.")]
    public float value;
    [Tooltip("How much damage is carried over to the next target when hitting multiple targets.\nWhen set to 0, only 1 target will be hit.")]
    [Range(0f, 1f)]
    public float piercingFactor;
    [Tooltip("How much (in degrees) a ranged weapon's shot will deviate from the aimed point.")]
    [Range(0f, 90f)]
    public float shotSpread;
    [Tooltip("Status Effects applied when item is used/equipped")]
    public StatusEffectTemplate statusEffect;
    [Tooltip("Tags used when checking skills, attributes, etc.")]
    public ItemTag[] tags;
    [Tooltip("The list of items (and quantities) required to craft this item")]
    public RecipeRequirement[] recipeReqs;
    [Tooltip("How this piece of equipment will modify the visual appearance")]
    public SpineEquipmentManager.EquipDef[] equipDefs;


    //Helper Methods
    bool isMeleeWeaponInTemplate()
    {
        return weaponTemplate.weaponType == WeaponTemplate.WeaponTypes.Melee;
    }
    public bool isWeapon()
    {
        return itemType == ItemTypes.Weapon;
    }
    public bool isMeleeWeapon()
    {
        if (weaponTemplate != null)
            return isMeleeWeaponInTemplate();
        else
        {
            Debug.LogError(name + ": has no weaponTemplate assigned during isMeleeWeapon() check");
            return false;
        }
    }
    public bool isRangedWeapon()
    {
        if (weaponTemplate != null)
            return isMeleeWeaponInTemplate();
        else
        {
            Debug.LogError(name + ": has no weaponTemplate assigned during isRangedWeapon() check");
            return false;
        }
    }
    public bool isThrowingWeapon()
    {
        return itemType == ItemTypes.Throwable;
    }
    public bool isApparel()
    {
        return itemType == ItemTypes.Apparel;
    }
    public bool isAmmo()
    {
        return itemType == ItemTypes.Ammo;
    }
    public bool isConsumable()
    {
        return itemType == ItemTypes.Consumable;
    }
    public bool isBook()
    {
        return itemType == ItemTypes.Book;
    }

    //IStatusEffectSource Methods
    StatusEffectTemplate IStatusEffectSource.GetTemplate()
    {
        return statusEffect;
    }
}

public class ItemSortBySaveID : System.Collections.Generic.Comparer<ItemTemplate>
{
    public override int Compare(ItemTemplate x, ItemTemplate y)
    {
        return x.saveID.CompareTo(y.saveID);
    }
}