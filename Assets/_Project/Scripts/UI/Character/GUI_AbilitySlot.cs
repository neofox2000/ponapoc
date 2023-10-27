using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;

public class GUI_AbilitySlot : MonoBehaviour,
    ISubmitHandler, ICancelHandler, ISelectHandler, 
    IPointerEnterHandler, IPointerUpHandler
{
    [Tooltip("The type of abilities that can be assigned")]
    public Image backIcon;
    public Image icon;

    BaseAbility dataRef = null;
    Selectable selectable = null;
    public int slotIndex { get; set; }
    public bool interactable
    {
        get { return selectable.interactable; }
        set { selectable.interactable = value; }
    }

    //Subscriber(s): HUD_Connector? (Purpose: update player controller)
    public Action<GUI_AbilitySlot, BaseAbility> OnChanged;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_AbilitySlot> OnUsed;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_AbilitySlot> OnClear;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_AbilitySlot> OnSelected;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }
    public void Set(BaseAbility newRef)
    {
        if (dataRef != newRef)
        {
            dataRef = newRef;
            UpdateDisplay();

            //Fire event
            if (OnChanged != null)
                OnChanged(this, newRef);
        }
    }
    void UpdateDisplay()
    {
        bool haveRef = dataRef != null;

        //Set
        backIcon.gameObject.SetActive(!haveRef);
        icon.gameObject.SetActive(haveRef);

        if (haveRef)
            Common.SetupIcon(icon, dataRef.template.icon, Color.white);
    }
    
    //UI Event Handlers
    public void OnSubmit(BaseEventData eventData)
    {
        //Fire Event
        if (OnUsed != null)
            OnUsed(this);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //Left-click = submit
        if (eventData.button == PointerEventData.InputButton.Left)
            OnSubmit(eventData);
        //Right-click = cancel
        if (eventData.button == PointerEventData.InputButton.Right)
            OnCancel(eventData);
    }
    public void OnCancel(BaseEventData eventData)
    {
        //Fire Event
        if (OnClear != null)
            OnClear(this);
    }
    public void OnSelect(BaseEventData eventData)
    {
        //Fire event
        if (OnSelected != null)
            OnSelected(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSelect(eventData);
    }
}