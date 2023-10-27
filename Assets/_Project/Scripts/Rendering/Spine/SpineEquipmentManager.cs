using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;

public class SpineEquipmentManager : MonoBehaviour
{
    public bool runApply = false;

    public enum EquipSlotNames
    {
        None,
        Weapon,
        WeaponClip,
        Torso,
        Throwable,
        Unused2,
        Unused3,
        Head,
        Ear,
        Neck,
        Horn,
        Muzzle,
        Eye,
        Mane,
        Unused4,
        Tail,
        ArmUpperRight,
        ArmLowerRight,
        ArmUpperLeft,
        ArmLowerLeft,
        LegUpperRight,
        LegLowerRight,
        LegUpperLeft,
        LegLowerLeft,
        FootRight,
        FootLeft
    }

    [System.Serializable]
    public class CoveredSlots
    {
        [SpineSlot]
        public string slot;
        [HideInInspector]
        public string normalRegion;
    }

    [System.Serializable]
    public class EquipmentSpineSlot
    {
        [SpineSlot]
        public string slot;
        public EquipSlotNames slotType;
        public List<CoveredSlots> coverdSlots;

        bool _dirty = false;
        string _region;
        string region
        {
            get { return _region; }
            set
            {
                if(_region != value)
                {
                    _region = value;
                    _dirty = true;
                }
            }
        }

        BaseItem _equippedItem = null;
        public BaseItem equippedItem
        {
            get { return _equippedItem; }
            set { _equippedItem = value; }
                /*
                set
                {
                    if (_equippedItem != value)
                    {
                        if (_equippedItem != null)
                            _equippedItem.equipped = false;

                        _equippedItem = value;
                        if (_equippedItem != null)
                            _equippedItem.equipped = true;
                    }
                }*/
            }

        public void fill(string region)
        {
            this.region = region;
        }
        public void clear()
        {
            region = string.Empty;
        }
        void setCoveredSlotState(NativeRegionAttacher nativeRegionAttacher, bool hidden)
        {
            foreach (CoveredSlots cs in coverdSlots)
                nativeRegionAttacher.Apply(
                    cs.slot, 
                    hidden ? string.Empty : cs.normalRegion);
        }
        public void apply(NativeRegionAttacher nativeRegionAttacher)
        {
            if (_dirty)
            {
                setCoveredSlotState(
                    nativeRegionAttacher, 
                    region != string.Empty);

                nativeRegionAttacher.Apply(slot, region);
                _dirty = false;
            }
        }
        public void init(NativeRegionAttacher nativeRegionAttacher)
        {
            foreach (CoveredSlots CS in coverdSlots)
                CS.normalRegion = nativeRegionAttacher.getRegion(CS.slot);
        }
    }

    [System.Serializable]
    public class EquipDef
    {
        public EquipSlotNames slot;
        public string region;
        public Color32 color;

        public EquipDef(EquipSlotNames slot, string region, Color32 color)
        {
            this.slot = slot;
            this.region = region;
            this.color = color;
        }
    }

    public EquipmentSpineSlot[] slots;

    //SkeletonRenderer skeletonRenderer = null;
    NativeRegionAttacher nativeRegionAttacher = null;

    void Awake()
    {
        //skeletonRenderer = GetComponentInChildren<SkeletonRenderer>();
        nativeRegionAttacher = GetComponent<NativeRegionAttacher>();
        if (nativeRegionAttacher == null)
        {
            nativeRegionAttacher = gameObject.AddComponent<NativeRegionAttacher>();
            nativeRegionAttacher.Awake();
        }
    }
    void Start()
    {
        Init();
        Apply();
    }
    void FixedUpdate()
    {
        if (runApply)
        {
            runApply = false;
            Apply();
        }
    }

    public EquipDef getEquipDef(BaseItem item, EquipSlotNames slotType)
    {
        foreach (EquipDef equipDef in item.template.equipDefs)
            if (equipDef.slot == slotType)
                return equipDef;

        return null;
    }
    public EquipmentSpineSlot getSlot(EquipSlotNames slotType)
    {
        EquipmentSpineSlot ret = null;

        //Cache the slot positions in the equipslot array
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].slotType == slotType)
            {
                ret = slots[i];
                break;
            }
        }
        return ret;
    }

    public void Init()
    {
        foreach (EquipmentSpineSlot ESS in slots)
            ESS.init(nativeRegionAttacher);
    }
    public void Apply()
    {
        foreach (EquipmentSpineSlot slot in slots)
            slot.apply(nativeRegionAttacher);
    }
    public void switchEquipment(int slotIndex, string region)
    {
        EquipmentSpineSlot slot = slots[slotIndex];
        slot.fill(region);
        Apply();
    }
    public BaseItem[] getOccupiedSlotItems(BaseItem item)
    {
        EquipmentSpineSlot slot;
        List<BaseItem> itemList = new List<BaseItem>();
        if (item.template.equipDefs != null)
            foreach (EquipDef equipDef in item.template.equipDefs)
            {
                slot = getSlot(equipDef.slot);
                if (slot != null)
                {
                    if ((slot.equippedItem != null) && (!itemList.Contains(slot.equippedItem)))
                        itemList.Add(slot.equippedItem);
                }
                else Debug.LogWarning("No available slot for " + item.template.name);
            }

        return itemList.ToArray();
    }
    public bool equipItem(BaseItem item, bool putItOn = true)
    {
        EquipmentSpineSlot slot;

        foreach (EquipDef equipDef in item.template.equipDefs)
        {
            slot = getSlot(equipDef.slot);
            if (slot == null)
            {
                Debug.LogWarning("Character does not have this type of slot!");
                return false;
            }

            if (putItOn)
            {
                slot.equippedItem = item;
                slot.fill(equipDef.region);
            }
            else
            {
                slot.equippedItem = null;
                slot.clear();
            }
        }

        //Update visuals
        //Apply();

        return true;
    }
    public void clearSlots()
    {
        foreach (EquipmentSpineSlot slot in slots)
            slot.clear();
    }
}