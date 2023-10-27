using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GUI_Sheet : MonoBehaviour
{
    //Inspector Properties
    [SerializeField] protected ScrollRect scrollRect;

    //Protected Properties
    //protected BaseController controllerLink;
    [SerializeField] protected CharacterSheet characterSheetLink;
    [SerializeField] protected Inventory inventoryLink;

    //Accessors
    protected bool refresh { get; set; }
    public bool editMode { get; set; }

    //Events
    public Action OnEditModeChanged;

    //Mono Methods
    protected virtual void Update()
    {
        if (refresh && (gameObject.activeSelf))
        {
            //if (characterSheetLink != null)
                Refresh();
            //else Debug.LogWarning("Trying to refresh sheet without a characterSheet Link");
        }

        refresh = false;
    }
    protected virtual void OnEnable()
    {
        refresh = true;
        AddEvents();
    }
    protected virtual void OnDisable()
    {
        RemoveEvents();
    }

    protected virtual void Refresh()
    {
        //Override to populate sheet
    }
    protected virtual void AddEvents()
    {
        //eventsAreOn = true;
    }
    protected virtual void RemoveEvents()
    {
        //eventsAreOn = false;
    }
    protected virtual void OnChanged()
    {
        if(gameObject.activeSelf)
            refresh = true;
    }
    protected virtual void UpdateEditMode(bool newEditMode)
    {
        editMode = newEditMode;

        //Notify other stuff of change in edit mode
        if(OnEditModeChanged != null)
            OnEditModeChanged();
    }
    public virtual void Scroll(BaseEventData eventData)
    {
        if(scrollRect)
            scrollRect.OnScroll((PointerEventData)eventData);
    }
}
