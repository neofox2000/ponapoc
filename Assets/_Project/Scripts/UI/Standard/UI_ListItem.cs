using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Selectable))]
public class UI_ListItem : MonoBehaviour,
    IScrollHandler, ISelectHandler, IDeselectHandler,
    ICancelHandler, ISubmitHandler, IPointerEnterHandler, IPointerUpHandler
{
    public RectTransform rTrans { get; protected set; }
    public Selectable selectable { get; protected set; }

    IListRowSource _sourceLink = null;
    public IListRowSource sourceLink
    {
        get { return _sourceLink; }
        set
        {
            _sourceLink = value;
            SetupDisplay();
        }
    }


    //Events
    public Action<UI_ListItem, SubmitState> OnSubmitted;
    public Action<UI_ListItem> OnCanceled;
    public Action<UI_ListItem> OnSelected;
    public Action OnDeselected;
    public Action<PointerEventData> OnScrolled;

    //Mono methods
    protected virtual void Awake()
    {
        rTrans = GetComponent<RectTransform>();
        selectable = GetComponent<Selectable>();
    }
    protected virtual void Start()
    {
        SetupDisplay();
    }

    //Event Handlers
    public virtual void OnSubmit(BaseEventData eventData)
    {
        //Ignore other mouse click types
        if ((eventData is PointerEventData) && (((PointerEventData)eventData).button != PointerEventData.InputButton.Left))
            return;

        if (OnSubmitted != null)
            OnSubmitted(
                this, 
                Common.getKeyboardSubmitState());
    }
    public virtual void OnCancel(BaseEventData eventData)
    {
        //Fire event
        if (OnCanceled != null)
            OnCanceled(this);
    }
    public virtual void OnSelect(BaseEventData eventData)
    {
        if (OnSelected != null)
            OnSelected(this);
    }
    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (OnDeselected != null)
            OnDeselected();
    }
    public virtual void OnScroll(PointerEventData eventData)
    {
        if (OnScrolled != null)
            OnScrolled(eventData);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(selectable.interactable)
            selectable.Select();
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
            OnSubmit(eventData);
        if (eventData.button == PointerEventData.InputButton.Right)
            OnCancel(eventData);
    }

    //Main Methods
    public virtual void SetupDisplay()
    {
        //Placeholder
    }
}