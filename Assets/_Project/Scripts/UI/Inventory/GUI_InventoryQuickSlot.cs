using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RPGData;

public class GUI_InventoryQuickSlot : MonoBehaviour, 
    ISubmitHandler, IPointerUpHandler, ICancelHandler, ISelectHandler, IPointerEnterHandler
{
    public ItemTypes slotType;
    public Image backIcon;
    public Image icon;

    BaseItem dataRef = null;
    Selectable selectable = null;
    RectTransform rTrans = null;
    public int slotIndex { get; set; }

    public bool interactable
    {
        get { return selectable.interactable; }
        set { selectable.interactable = value; }
    }

    //Subscriber(s): HUD_Connector? (Purpose: update player controller)
    public Action<GUI_InventoryQuickSlot, BaseItem> OnChanged;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_InventoryQuickSlot> OnUsed;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_InventoryQuickSlot> OnClear;
    //Subscriber(s): GUI_InventoryQuickSlotsManager
    public Action<GUI_InventoryQuickSlot> OnSelected;

    //Core Methods
    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        rTrans = GetComponent<RectTransform>();
    }
    public void Set(BaseItem newRef)
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
        //Is an item assigned to this slot?
        if (dataRef == null)
        {
            //Hide item details
            GUI_Common.instance.HideItemDetails();

            //Show basic details
            GUI_Common.instance.ShowDetailsPopup(
                slotType.ToString() + " Quickslot: Empty", rTrans,
                TextAlignment.Left);
        }
        else
        {
            //Hide basic details
            GUI_Common.instance.HideDetailsPopup();

            //Show item details from quickslot
            GUI_Common.instance.ShowItemDetails(
                dataRef, 
                rTrans, 
                TextAlignment.Left);
        }

        //Fire event
        if (OnSelected != null)
            OnSelected(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //OnSelect(eventData);
        if (selectable.interactable)
            selectable.Select();
    }
}