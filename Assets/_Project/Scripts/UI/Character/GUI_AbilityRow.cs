using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GUI_AbilityRow : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler,
    ISubmitHandler, ICancelHandler,
    IScrollHandler, IDragHandler, 
    ISelectHandler, IDeselectHandler
{
    public Text displayName, theoryText, practiceText;
    public Image abilityIcon, theoryBar, practiceBar;

    public BaseAbility dataRef { get; protected set; }
    public Selectable selectable { get; protected set; }

    //Events
    public Action<GUI_AbilityRow> OnSubmitAction;
    public Action<GUI_AbilityRow> OnCancelAction;
    public Action<GUI_AbilityRow> OnSelectAction;
    public Action<GUI_AbilityRow> OnDeselectAction;
    public Action<PointerEventData> OnScrollAction;
    public Action<PointerEventData, GUI_AbilityRow> OnDragAction;


    //Mono
    void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    //Initialization
    void setupName(string newName, Color newColor)
    {
        displayName.text = newName;
        displayName.color = newColor;
    }
    void setupValue(BaseAbility ability)
    {
        theoryText.text = ability.theoryLevel.valueBase.ToString("0");
        practiceText.text = ability.practiceLevel.valueBase.ToString("0");

        theoryBar.fillAmount = ability.theoryXP / ability.requiredTheoryXP();
        practiceBar.fillAmount = ability.practiceXP / ability.requiredPracticeXP();
    }
    void setupIcon(Sprite sprite)
    {
        Common.SetupIcon(abilityIcon, sprite, Color.white);        
    }
    /// <summary>
    /// Call with null value to disconnect
    /// </summary>
    /// <param name="dataRef"></param>
    /// <param name="sheetRef"></param>
    public void Setup(BaseAbility dataRef)
    {
        if (dataRef != this.dataRef)
        {
            this.dataRef = dataRef;
            if (this.dataRef != null)
            {
                setupName(this.dataRef.getDisplayName(), Color.white);
                setupIcon(this.dataRef.template.icon);
            }
            refresh();
        }
    }
    
    //Upkeep
    public void refresh()
    {
        if (dataRef != null)
            setupValue(dataRef);
    }

    //Generic Events
    public void OnSubmit(BaseEventData eventData)
    {
        if (OnSubmitAction != null)
            OnSubmitAction(this);
    }
    public void OnCancel(BaseEventData eventData)
    {
        if (OnCancelAction != null)
            OnCancelAction(this);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (OnSelectAction != null)
            OnSelectAction(this);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (OnDeselectAction != null)
            OnDeselectAction(this);
    }
    public void OnScroll(PointerEventData eventData)
    {
        if (OnScrollAction != null)
            OnScrollAction(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragAction != null)
            OnDragAction(eventData, this);
    }

    //Mouse events
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSelect(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnDeselect(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        OnSubmit(eventData);
    }
}
