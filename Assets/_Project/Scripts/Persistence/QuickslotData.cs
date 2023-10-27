using UnityEngine;
using System;
using RPGData;

[Serializable]
public class QuickslotData
{
    #region Constants
    public const int weaponSlotCount = 3;
    public const int throwableSlotCount = 3;
    public const int consumableSlotCount = 8;
    public const int abilitySlotCount = 4;
    const int slotTypeCount = 4;
    const char arraySeperator = ';';
    const char indexSeperator = ',';
    #endregion

    #region Data
    //Weapons Data
    ItemTemplate[] weapons;
    int _activeWeaponSlot = 0;
    public int activeWeaponSlot
    {
        get { return _activeWeaponSlot; }
        set
        {
            _activeWeaponSlot = value;
            if (OnActiveWeaponSlotChanged != null)
                OnActiveWeaponSlotChanged(value);
        }
    }

    //Weapons Data
    ItemTemplate[] throwables;
    int _activeThrowableSlot = 0;
    public int activeThrowableSlot
    {
        get { return _activeThrowableSlot; }
        set
        {
            _activeThrowableSlot = value;
            if (OnActiveThrowableSlotChanged != null)
                OnActiveThrowableSlotChanged(value);
        }
    }

    //Weapons Data
    ItemTemplate[] consumables;
    int _activeConsumableSlot = 0;
    public int activeConsumableSlot
    {
        get { return _activeConsumableSlot; }
        set
        {
            _activeConsumableSlot = value;

            //Fire event
            if (OnActiveConsumableSlotChanged != null)
                OnActiveConsumableSlotChanged(value);
        }
    }

    //Weapons Data
    AbilityTemplate[] abilities;
    int _activeAbilitySlot = 0;
    public int activeAbilitySlot
    {
        get { return _activeAbilitySlot; }
        set
        {
            _activeAbilitySlot = value;
            if (OnActiveAbilitySlotChanged != null)
                OnActiveAbilitySlotChanged(value);
        }
    }
    #endregion

    #region Cache
    //These values are all set and used externally.
    //They are here as a central access point for sharing references.
    public BaseItem activeWeaponRef { get; set; }
    public BaseItem activeWeaponAmmoRef { get; set; }
    public BaseItem activeThrowableRef { get; set; }
    public BaseItem activeConsumableRef { get; set; }
    public BaseAbility activeAbilityRef { get; set; }
    #endregion

    #region Events
    public Action<int, ItemTemplate> OnWeaponSlotChanged;
    public Action<int> OnActiveWeaponSlotChanged;
    public Action<int, ItemTemplate> OnThrowableSlotChanged;
    public Action<int> OnActiveThrowableSlotChanged;
    public Action<int, ItemTemplate> OnConsumableSlotChanged;
    public Action<int> OnActiveConsumableSlotChanged;
    public Action<int, AbilityTemplate> OnAbilitySlotChanged;
    public Action<int> OnActiveAbilitySlotChanged;
    #endregion

    #region Methods
    //Constructors
    public QuickslotData()
    {
        CreateEmpty();
    }
    public QuickslotData(string serializedData)
    {
        //Initialize the slot data in the default all-empty state
        //If the data is bad or missing, this will be the fallback state
        CreateEmpty();

        //Try deserializing
        DeserializeFrom(serializedData);
    }
    /// <summary>
    ///Initializes the slot data in the default all-empty state
    /// </summary>
    void CreateEmpty()
    {
        //Clear cache
        activeWeaponRef = null;
        activeWeaponAmmoRef = null;
        activeThrowableRef = null;
        activeConsumableRef = null;
        activeAbilityRef = null;

        //Init weapon slots
        weapons = new ItemTemplate[weaponSlotCount];
        for (int i = 0; i < weapons.Length; i++)
            weapons[i] = null;

        //Init throwable slots
        throwables = new ItemTemplate[throwableSlotCount];
        for (int i = 0; i < throwables.Length; i++)
            throwables[i] = null;

        //Init consumable slots
        consumables = new ItemTemplate[consumableSlotCount];
        for (int i = 0; i < consumables.Length; i++)
            consumables[i] = null;

        //Init ability slots
        abilities = new AbilityTemplate[abilitySlotCount];
        for (int i = 0; i < abilities.Length; i++)
            abilities[i] = null;
    }

    //Serialization
    void DeserializeSlotArray(ref int[] slotArray, string data)
    {
        if (data != string.Empty)
        {
            //Split data
            string[] intArray = data.Split(indexSeperator);

            //Convert data to int
            for (int i = 0; i < Mathf.Min(intArray.Length, slotArray.Length); i++)
                slotArray[i] = int.TryParse(intArray[i], out slotArray[i]) ? slotArray[i] : -1;
        }
    }
    void DeserializeFrom(string serializedData)
    {
        bool success = false;

        if (serializedData != string.Empty)
        {
            string[] dataArray = serializedData.Split(arraySeperator);
            int[] abilitySaveIds = new int[abilitySlotCount];
            int[] weaponSaveIds = new int[weaponSlotCount];
            int[] throwableSaveIds = new int[throwableSlotCount];
            int[] consumableSaveIds = new int[consumableSlotCount];
            if (dataArray.Length == slotTypeCount + 1)
            {
                DeserializeSlotArray(ref weaponSaveIds, dataArray[0]);
                DeserializeSlotArray(ref throwableSaveIds, dataArray[1]);
                DeserializeSlotArray(ref consumableSaveIds, dataArray[2]);
                DeserializeSlotArray(ref abilitySaveIds, dataArray[3]);

                weapons = SetItemSlotsFromSaveIdArray(weaponSaveIds);
                throwables = SetItemSlotsFromSaveIdArray(throwableSaveIds);
                consumables = SetItemSlotsFromSaveIdArray(consumableSaveIds);
                abilities = SetAbilitySlotsFromSaveIdArray(abilitySaveIds);

                try
                {
                    string[] activeSlotIndexes = dataArray[4].Split(indexSeperator);
                    activeWeaponSlot = int.Parse(activeSlotIndexes[0]);
                    activeThrowableSlot = int.Parse(activeSlotIndexes[1]);
                    activeConsumableSlot = int.Parse(activeSlotIndexes[2]);
                    activeAbilitySlot = int.Parse(activeSlotIndexes[3]);
                }
                catch
                {
                    Debug.LogError("Bleh");
                    activeWeaponSlot = 0;
                    activeThrowableSlot = 0;
                    activeConsumableSlot = 0;
                    activeAbilitySlot = 0;
                }

                success = true;
            }
        }

        //Did the data get deserialized correctly?
        if (!success)
        {
            //Create a log warning
            Debug.LogWarning("Bad or missing quickslot data - creating new");
        }
    }
    string SerializeSlotArray(int[] slotArray)
    {
        string[] stringArray = new string[slotArray.Length];
        for (int i = 0; i < slotArray.Length; i++)
            stringArray[i] = slotArray[i].ToString();

        return string.Join(indexSeperator.ToString(), stringArray);
    }
    int[] GetAbilitySaveIdArray(AbilityTemplate[] abilities)
    {
        int[] ret = new int[abilities.Length];
        for (int i = 0; i < abilities.Length; i++)
            ret[i] = abilities[i] == null ? -1 : abilities[i].saveID;
        return ret;
    }
    int[] GetItemSaveIdArray(ItemTemplate[] items)
    {
        int[] ret = new int[items.Length];
        for (int i = 0; i < items.Length; i++)
            ret[i] = items[i] == null ? -1 : items[i].saveID;
        return ret;
    }
    AbilityTemplate[] SetAbilitySlotsFromSaveIdArray(int[] array)
    {
        AbilityTemplate[] ret = new AbilityTemplate[array.Length];

        //Find abilities by their save ID from the game's database list
        for (int i = 0; i < array.Length; i++)
            ret[i] = Array.Find(GameDatabase.core.abilities, x => x.saveID == array[i]);

        return ret;
    }
    ItemTemplate[] SetItemSlotsFromSaveIdArray(int[] array)
    {
        ItemTemplate[] ret = new ItemTemplate[array.Length];

        //Find abilities by their save ID from the game's database list
        for (int i = 0; i < array.Length; i++)
            ret[i] = Array.Find(GameDatabase.core.items, x => x.saveID == array[i]);

        return ret;
    }
    public string SerializeMe()
    {
        string[] dataArray = new string[slotTypeCount + 1];
        dataArray[0] = SerializeSlotArray(GetItemSaveIdArray(weapons));
        dataArray[1] = SerializeSlotArray(GetItemSaveIdArray(throwables));
        dataArray[2] = SerializeSlotArray(GetItemSaveIdArray(consumables));
        dataArray[3] = SerializeSlotArray(GetAbilitySaveIdArray(abilities));
        dataArray[4] = string.Join(
            indexSeperator.ToString(), 
            new string[4] {
                activeWeaponSlot.ToString(),
                activeThrowableSlot.ToString(),
                activeConsumableSlot.ToString(),
                activeAbilitySlot.ToString()
            });

        return string.Join(arraySeperator.ToString(), dataArray);
    }

    //Accessibility
    int WrapInt(int currentValue, int maxValue)
    {
        if ((currentValue + 1) > maxValue)
            currentValue = 0;
        else
            currentValue++;

        return currentValue;
    }

    public bool SetItemQuickSlot(ItemTypes slotType, int slotIndex, ItemTemplate itemTemplate)
    {
        bool success = true;

        switch (slotType)
        {
            case ItemTypes.Weapon:
                weapons[slotIndex] = itemTemplate;

                //Fire Event
                if (OnWeaponSlotChanged != null)
                    OnWeaponSlotChanged(slotIndex, itemTemplate);
                break;
            case ItemTypes.Throwable:
                throwables[slotIndex] = itemTemplate;

                //Fire Event
                if (OnThrowableSlotChanged != null)
                    OnThrowableSlotChanged(slotIndex, itemTemplate);
                break;
            case ItemTypes.Consumable:
                consumables[slotIndex] = itemTemplate;

                //Fire Event
                if (OnConsumableSlotChanged != null)
                    OnConsumableSlotChanged(slotIndex, itemTemplate);
                break;
            default:
                success = false;
                break;
        }

        return success;
    }
    public bool SetActiveItemQuickSlot(ItemTypes slotType, ItemTemplate itemTemplate)
    {
        switch (slotType)
        {
            case ItemTypes.Weapon:
                return SetItemQuickSlot(slotType, activeWeaponSlot, itemTemplate);
            case ItemTypes.Throwable:
                return SetItemQuickSlot(slotType, activeThrowableSlot, itemTemplate);
            case ItemTypes.Consumable:
                return SetItemQuickSlot(slotType, activeConsumableSlot, itemTemplate);
        }

        return false;
    }

    public ItemTemplate GetWeaponSlot(int index)
    {
        return weapons[index];
    }
    public ItemTemplate GetActiveWeaponSlot()
    {
        return weapons[activeWeaponSlot];
    }
    public void CycleActiveWeaponSlot()
    {
        activeWeaponSlot = WrapInt(activeWeaponSlot, weaponSlotCount - 1);
    }

    public ItemTemplate GetThrowableSlot(int index)
    {
        return throwables[index];
    }
    public ItemTemplate GetActiveThrowableSlot()
    {
        return throwables[activeThrowableSlot];
    }
    public void CycleActiveThrowableSlot()
    {
        activeThrowableSlot = WrapInt(activeThrowableSlot, throwableSlotCount - 1);
    }

    public ItemTemplate GetConsumableSlot(int index)
    {
        return consumables[index];
    }
    public ItemTemplate GetActiveConsumableSlot()
    {
        return consumables[activeConsumableSlot];
    }
    public void CycleActiveConsumableSlot()
    {
        //Store temporary value to avoid triggering events
        int theChosenOne = activeConsumableSlot;

        if (AssignedConsumableSlotCount() > 1)
        {
            //Move to the next slot
            theChosenOne = WrapInt(theChosenOne, consumableSlotCount - 1);

            //Skip empty slots
            while (GetConsumableSlot(theChosenOne) == null)
                theChosenOne = WrapInt(theChosenOne, consumableSlotCount - 1);
        }
        else
        {
            //Go back to the first slot by default
            theChosenOne = 0;
        }

        //Set accessor value (and trigger event)
        activeConsumableSlot = theChosenOne;
    }
    public int AssignedConsumableSlotCount()
    {
        int c = 0;
        foreach (ItemTemplate it in consumables)
            if (it != null)
                c++;

        return c;
    }

    public AbilityTemplate GetAbilitySlot(int index)
    {
        return abilities[index];
    }
    public AbilityTemplate GetActiveAbilitySlot()
    {
        return abilities[activeAbilitySlot];
    }
    public void SetAbilitySlot(int index, AbilityTemplate template)
    {
        abilities[index] = template;

        //Fire Event
        if (OnAbilitySlotChanged != null)
            OnAbilitySlotChanged(index, template);
    }
    public void CycleActiveAbilitySlot()
    {
        //Store temporary value to avoid triggering events
        int theChosenOne = activeAbilitySlot;

        if (AssignedAbilitySlotCount() > 1)
        {
            //Move to the next slot
            theChosenOne = WrapInt(theChosenOne, abilitySlotCount - 1);

            //Skip empty slots
            while (GetAbilitySlot(theChosenOne) == null)
                theChosenOne = WrapInt(theChosenOne, abilitySlotCount - 1);
        }
        else
        {
            //Go back to the first slot by default
            theChosenOne = 0;
        }

        //Set accessor value (and trigger event)
        activeAbilitySlot = theChosenOne;
    }
    public int AssignedAbilitySlotCount()
    {
        int c = 0;
        foreach (AbilityTemplate template in abilities)
            if (template != null)
                c++;

        return c;
    }
    #endregion
}