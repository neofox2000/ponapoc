using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GUI_AbilitiesSheet : GUI_Sheet
{
    public GameObject rowPrefab;
    public GUI_AbilitySlot[] slots;
    List<GUI_AbilityRow> rows = null;
    GUI_AbilitySlot slotInFocus;

    bool assigningSlot { get { return slotInFocus != null; } }

    //Initialization methods
    private void Awake()
    {
        //Subscribe to slot events
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotIndex = i;
            AddSlotEvents(slots[i]);
        }
    }
    protected override void AddEvents()
    {
        base.AddEvents();

        characterSheetLink.OnAbilityAdded += AbilityAdded;
    }
    protected override void RemoveEvents()
    {
        base.RemoveEvents();

        if(characterSheetLink != null)
            characterSheetLink.OnAbilityAdded -= AbilityAdded;
    }
    void AddRowEvents(GUI_AbilityRow row)
    {
        row.OnSubmitAction += RowSubmited;
        row.OnCancelAction += RowCanceled;
        row.OnSelectAction += RowSelected;
        row.OnDeselectAction += RowDeselected;
        row.OnScrollAction += Scroll;
    }
    void RemoveRowEvents(GUI_AbilityRow row)
    {
        row.OnSubmitAction -= RowSubmited;
        row.OnCancelAction -= RowCanceled;
        row.OnSelectAction -= RowSelected;
        row.OnDeselectAction -= RowDeselected;
        row.OnScrollAction -= Scroll;
    }
    void AddSlotEvents(GUI_AbilitySlot slot)
    {
        slot.OnUsed += SlotUsed;
        slot.OnClear += SlotCleared;
        slot.OnSelected += SlotSelected;
    }
    public void CancelSlotAssignment(bool reselectSlot)
    {
        if (slotInFocus != null)
        {
            //Re-enable slot selection
            ToggleAllSlotsInteractable(true);

            //Switch active selection back to slot
            if (reselectSlot)
                EventSystem.current
                    .SetSelectedGameObject(slotInFocus.gameObject);

            //Exit assignment mode
            slotInFocus = null;
        }
    }
    //Row events
    void RowSubmited(GUI_AbilityRow row)
    {
        //Are we in assignment mode?
        if (assigningSlot)
        {
            //Assign ability to focused slot
            AssignAbilityToSlot(
                slotInFocus,
                row.dataRef);

            CancelSlotAssignment(true);
        }
    }
    void RowCanceled(GUI_AbilityRow row)
    {
        if(assigningSlot)
        {
            //TODO: Reset interactable stuff
            slotInFocus = null;
        }
    }
    void RowSelected(GUI_AbilityRow row)
    {
        //Show ability info panel
        GUI_Common.instance.ShowDetailsPopup(
            row.dataRef.getReadout(),
            row.GetComponent<RectTransform>(),
            TextAlignment.Left);
    }
    void RowDeselected(GUI_AbilityRow row)
    {
        //Hide ability info panel
        GUI_Common.instance.HideDetailsPopup();
    }
    void AbilityAdded(BaseAbility newAbility)
    {
        OnChanged();
    }

    //Slot events
    void SlotUsed(GUI_AbilitySlot slot)
    {
        //Make sure there is at least 1 skill to choose from
        if (rows.Count > 0)
        {
            //Go into assignment mode
            slotInFocus = slot;

            //Make all slots unselectable (to avoid confusing the player)
            ToggleAllSlotsInteractable(false);

            //Select the first ability
            rows[0].selectable.Select();
        }
    }
    void SlotCleared(GUI_AbilitySlot slot)
    {
        slot.Set(null);
        GameDatabase.sPlayerData.quickslotData.SetAbilitySlot(slot.slotIndex, null);
    }
    void SlotSelected(GUI_AbilitySlot slot)
    {
        //Do what?
    }
    void AssignAbilityToSlot(GUI_AbilitySlot slot, BaseAbility ability)
    {
        slot.Set(ability);
        GameDatabase.sPlayerData.quickslotData.SetAbilitySlot(slot.slotIndex, ability.template);
    }

    //Maintenance methods
    void ClearRows()
    {
        //Clean up existing rows
        if (rows != null)
        {
            foreach (GUI_AbilityRow row in rows)
            {
                RemoveRowEvents(row);
                row.Setup(null);
                if (row.gameObject != null)
                    Destroy(row.gameObject);
            }

            rows.Clear();
            rows = null;
        }
    }
    void SetupRows()
    {
        int rowCount = 0;
        if (characterSheetLink != null)
            rowCount = characterSheetLink.abilities.Count;

        //Setup abilities if there are any available
        if (rowCount > 0)
        {
            //Init rows
            if ((rows == null) || (rows.Count != rowCount))
            {
                //Clean up old rows
                ClearRows();

                //Create new rows
                rows = new List<GUI_AbilityRow>(rowCount);
                for (int i = 0; i < rowCount; i++)
                {
                    GameObject GO = Instantiate(rowPrefab) as GameObject;
                    GO.transform.SetParent(scrollRect.content.transform);

                    //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                    RectTransform RT = GO.GetComponent<RectTransform>();
                    RT.localScale = Vector3.one;

                    GUI_AbilityRow row = GO.GetComponent<GUI_AbilityRow>();
                    row.Setup(characterSheetLink.abilities[i]);
                    AddRowEvents(row);
                    rows.Add(row);
                }
            }
            else
            {
                //Refresh existing rows (no change in row count since last refresh)
                foreach (GUI_AbilityRow row in rows)
                    row.refresh();
            }
        }
        else
        {
            ClearRows();
        }
    }
    void SetupSlots()
    {
        //Setup Slots
        if (characterSheetLink != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Set(characterSheetLink.GetAbility(
                    GameDatabase.sPlayerData.quickslotData.GetAbilitySlot(slots[i].slotIndex)));
            }
        }
    }
    void ToggleAllSlotsInteractable(bool interactable)
    {
        foreach (GUI_AbilitySlot slot in slots)
            slot.interactable = interactable;
    }
    protected override void Refresh()
    {
        base.Refresh();

        SetupRows();
        SetupSlots();
    }
}