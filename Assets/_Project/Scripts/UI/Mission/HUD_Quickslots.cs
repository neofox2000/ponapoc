using UnityEngine;
using UnityEngine.UI;
using Variables;

public class HUD_Quickslots : MonoBehaviour
{
    [Header("Widgets")]
    public HUD_Quickslot weaponSlot;
    public Text weaponAmmoInBag;
    public HUD_Quickslot throwableSlot;
    public HUD_Quickslot consumableSlot;
    public HUD_Quickslot abilitySlot;

    bool weaponUsesAmmo = false;
    BaseItem weaponLink
    {
        get { return GameDatabase.sPlayerData.quickslotData.activeWeaponRef; }
        set { GameDatabase.sPlayerData.quickslotData.activeWeaponRef = value; }
    }
    BaseItem weaponAmmoLink
    {
        get { return GameDatabase.sPlayerData.quickslotData.activeWeaponAmmoRef; }
        set { GameDatabase.sPlayerData.quickslotData.activeWeaponAmmoRef = value; }
    }
    BaseItem throwableLink
    {
        get { return GameDatabase.sPlayerData.quickslotData.activeThrowableRef; }
        set { GameDatabase.sPlayerData.quickslotData.activeThrowableRef = value; }
    }
    BaseItem consumableLink
    {
        get { return GameDatabase.sPlayerData.quickslotData.activeConsumableRef; }
        set { GameDatabase.sPlayerData.quickslotData.activeConsumableRef = value; }
    }
    BaseAbility abilityLink
    {
        get { return GameDatabase.sPlayerData.quickslotData.activeAbilityRef; }
        set { GameDatabase.sPlayerData.quickslotData.activeAbilityRef = value; }
    }

    //Mono
    void OnEnable()
    {
        GameDatabase.sPlayerData.quickslotData.OnWeaponSlotChanged += WeaponSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnThrowableSlotChanged += ThrowableSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnConsumableSlotChanged += ConsumableSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnAbilitySlotChanged += AbilitySlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnActiveWeaponSlotChanged += ActiveWeaponSlotChange;
        GameDatabase.sPlayerData.quickslotData.OnActiveThrowableSlotChanged += ActivateThrowableSlotChange;
        GameDatabase.sPlayerData.quickslotData.OnActiveConsumableSlotChanged += ActivateConsumableSlot;
        GameDatabase.sPlayerData.quickslotData.OnActiveAbilitySlotChanged += ActivateAbilitySlot;

        //Subscribe to change events
        GameDatabase.lInventory.OnSingleItemChanged += PlayerItemChanged;
        GameDatabase.lInventory.OnMultipleItemsChanged += PlayerInventoryChanged;

        //Initialize slots
        SetWeaponSlot();
        SetThrowableSlot();
        SetConsumableSlot();
        SetAbilitySlot();
    }
    void OnDisable()
    {
        //Subscribe to change events
        GameDatabase.lInventory.OnSingleItemChanged -= PlayerItemChanged;
        GameDatabase.lInventory.OnMultipleItemsChanged -= PlayerInventoryChanged;

        //Clear slots
        ClearAll();

        //Unsubscribe from change events
        GameDatabase.sPlayerData.quickslotData.OnWeaponSlotChanged -= WeaponSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnThrowableSlotChanged -= ThrowableSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnConsumableSlotChanged -= ConsumableSlotDataChanged;
        GameDatabase.sPlayerData.quickslotData.OnAbilitySlotChanged -= AbilitySlotDataChanged;
    }
    void Update()
    {
        //Update weapon readouts
        if ((weaponLink != null) && (weaponUsesAmmo))
        {
            weaponSlot.SetReadout(weaponLink.chargesLeft.ToString());
            UpdateWeaponAmmoReadout();
        }

        //Update throwable readouts
        if (throwableLink != null)
            throwableSlot.SetReadout(throwableLink.quantity.ToString());

        //Update consumable readouts
        if (consumableLink != null)
            consumableSlot.SetReadout(consumableLink.quantity.ToString());

        //Update ability readout
        if (abilityLink != null)
        {
            abilitySlot.SetReadout(abilityLink.cooldownTimer > 0 ?
                Mathf.CeilToInt(abilityLink.cooldownTimer).ToString() :
                string.Empty);
            abilitySlot.SetProgress(1 - (abilityLink.cooldownTimer / abilityLink.template.cooldown));
        }
    }

    void ClearAll()
    {
        weaponSlot.SetIcon(null, Color.white);
        throwableSlot.SetIcon(null, Color.white);
        consumableSlot.SetIcon(null, Color.white);
        abilitySlot.SetIcon(null, Color.white);
    }
    void PlayerItemChanged(BaseItem item, bool removed)
    {
        if(removed)
            PlayerInventoryChanged();
    }
    void PlayerInventoryChanged()
    {
        RefreshAll();
    }
    void RefreshAll()
    {
        SetWeaponSlot();
        SetThrowableSlot();
        SetConsumableSlot();
        SetAbilitySlot();
    }

    //Weapon Slot handling
    void WeaponSlotDataChanged(int index, ItemTemplate itemTemplate)
    {
        if (index == GameDatabase.sPlayerData.quickslotData.activeWeaponSlot)
            SetWeaponSlot(itemTemplate);
    }
    void SetWeaponSlot(ItemTemplate itemTemplate)
    {
        weaponUsesAmmo = false;

        if (itemTemplate != null)
        {
            weaponLink = GameDatabase.lInventory.FindItem(itemTemplate);
            if (weaponLink != null)
            {
                //Show icon with normal color
                weaponSlot.SetIcon(
                    weaponLink.template.icon,
                    weaponLink.template.iconColor);

                weaponUsesAmmo = weaponLink.template.chargeUse > 0;
                if (weaponUsesAmmo)
                {
                    //Show ammo left in weapon
                    weaponSlot.SetReadout(weaponLink.chargesLeft.ToString());

                    //Cache ammo item stack
                    weaponAmmoLink = GameDatabase.lInventory.FindAmmo(weaponLink.template.chargeType);
                }
                else
                {
                    //Turn off readouts for melee weapons
                    weaponSlot.SetReadout(string.Empty);
                }
            }
            else
            {
                if (itemTemplate != null)
                {
                    float darkMultiplier = 0.2f;
                    weaponSlot.SetIcon(
                        itemTemplate.icon,
                        new Color(
                            itemTemplate.iconColor.r * darkMultiplier,
                            itemTemplate.iconColor.g * darkMultiplier,
                            itemTemplate.iconColor.b * darkMultiplier,
                            1f));
                    weaponSlot.SetReadout(string.Empty);
                }
                else
                    Debug.LogError("Could not find itemID " + itemTemplate + "!");
            }
        }
        else
        {
            weaponLink = null;
            weaponAmmoLink = null;
            weaponSlot.Clear();
        }

        UpdateWeaponAmmoReadout();
    }
    void SetWeaponSlot()
    {
        SetWeaponSlot(GameDatabase.sPlayerData.quickslotData.GetActiveWeaponSlot());
    }
    void UpdateWeaponAmmoReadout()
    {
        if (weaponUsesAmmo)
        {
            weaponAmmoInBag.gameObject.SetActive(true);

            weaponAmmoInBag.text = weaponAmmoLink != null ?
                weaponAmmoLink.quantity.ToString() :
                weaponAmmoInBag.text = "0";
        }
        else
        {
            weaponAmmoInBag.gameObject.SetActive(false);
        }
    }
    void ActiveWeaponSlotChange(int slotID)
    {
        weaponSlot.Flash();
        SetWeaponSlot();
    }

    //Throwable Slot handling
    void ThrowableSlotDataChanged(int index, ItemTemplate itemTemplate)
    {
        if (index == GameDatabase.sPlayerData.quickslotData.activeThrowableSlot)
            SetThrowableSlot(itemTemplate);
    }
    void SetThrowableSlot(ItemTemplate itemTemplate)
    {
        if (itemTemplate != null)
        {
            throwableLink = GameDatabase.lInventory.FindItem(itemTemplate);
            if (throwableLink != null)
            {
                //Show icon with normal color
                throwableSlot.SetIcon(
                    throwableLink.template.icon,
                    throwableLink.template.iconColor);

                //Show amount remaining
                throwableSlot.SetReadout(throwableLink.quantity.ToString());
            }
            else
            {
                float darkMultiplier = 0.2f;
                throwableSlot.SetIcon(
                    itemTemplate.icon,
                    new Color(
                        itemTemplate.iconColor.r * darkMultiplier,
                        itemTemplate.iconColor.g * darkMultiplier,
                        itemTemplate.iconColor.b * darkMultiplier,
                        1f));
                throwableSlot.SetReadout(string.Empty);
            }
        }
        else
        {
            throwableLink = null;
            throwableSlot.Clear();
        }
    }
    void SetThrowableSlot()
    {
        SetThrowableSlot(GameDatabase.sPlayerData.quickslotData.GetActiveThrowableSlot());
    }
    void ActivateThrowableSlotChange(int slotID)
    {
        throwableSlot.Flash();
        SetThrowableSlot();
    }

    //Consumable Slot handling
    void ConsumableSlotDataChanged(int index, ItemTemplate itemTemplate)
    {
        if (index == GameDatabase.sPlayerData.quickslotData.activeConsumableSlot)
            SetConsumableSlot(itemTemplate);
    }
    void SetConsumableSlot(ItemTemplate itemTemplate)
    {
        if (itemTemplate != null)
        {
            consumableLink = GameDatabase.lInventory.FindItem(itemTemplate);
            if (consumableLink != null)
            {
                //Show icon with normal color
                consumableSlot.SetIcon(
                    consumableLink.template.icon,
                    consumableLink.template.iconColor);

                //Show amount remaining
                consumableSlot.SetReadout(consumableLink.quantity.ToString());
            }
            else
            {
                float darkMultiplier = 0.2f;
                consumableSlot.SetIcon(
                    itemTemplate.icon,
                    new Color(
                        (itemTemplate.iconColor.r * darkMultiplier) / 256,
                        (itemTemplate.iconColor.g * darkMultiplier) / 256,
                        (itemTemplate.iconColor.b * darkMultiplier) / 256,
                        1f));
                consumableSlot.SetReadout(string.Empty);
            }
        }
        else
        {
            consumableLink = null;
            consumableSlot.Clear();
        }
    }
    void SetConsumableSlot()
    {
        SetConsumableSlot(GameDatabase.sPlayerData.quickslotData.GetActiveConsumableSlot());
    }
    void ActivateConsumableSlot(int slotID)
    {
        consumableSlot.Flash();
        SetConsumableSlot();
    }

    //Ability Slot handling
    void AbilitySlotDataChanged(int index, AbilityTemplate template)
    {
        if (index == GameDatabase.sPlayerData.quickslotData.activeAbilitySlot)
            SetAbilitySlot(template);
    }
    void SetAbilitySlot(AbilityTemplate template)
    {
        if (template != null)
        {
            abilityLink = GameDatabase.lCharacterSheet.GetAbility(template);
            abilitySlot.SetIcon(
                abilityLink.template.icon,
                abilityLink.template.color);
        }
        else
        {
            abilityLink = null;
            abilitySlot.Clear();
        }
    }
    void SetAbilitySlot()
    {
        SetAbilitySlot(GameDatabase.sPlayerData.quickslotData.GetActiveAbilitySlot());
    }
    void ActivateAbilitySlot(int slotID)
    {
        abilitySlot.Flash();
        SetAbilitySlot();
    }
}