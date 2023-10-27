using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RPGData;

[RequireComponent(typeof(Selectable))]
public class GUI_StatRow : MonoBehaviour, 
    IScrollHandler, ISelectHandler, IDeselectHandler,
    ICancelHandler, ISubmitHandler, IPointerEnterHandler
{
    public Text statName, baseValue, moddedValue;
    public RectTransform buttonsPanel;
    public Button addButton, subtractButton;
    public bool showBase = true;

    RectTransform myTrans;
    Attribute dataLink;
    Selectable selectable;

    //Events
    public System.Action<Attribute, RectTransform> OnSelected;
    public System.Action OnDeselected;
    public System.Action<Attribute, float> OnAddStat;
    public System.Action<Attribute, float> OnSubtractStat;
    public System.Action<BaseEventData> OnScrolled;

    //Initialization Methods
    void Awake()
    {
        myTrans = GetComponent<RectTransform>();
        selectable = GetComponent<Selectable>();
    }
    void SetupName()
    {
        statName.text = dataLink.template.GetDisplayName();
        statName.color = dataLink.template.color;
    }
    void SetupValue(float value, float valueModded)
    {
        baseValue.text = value.ToString("0");
        moddedValue.text = valueModded.ToString("(0)");
    }
    public void Setup(Attribute attribute, bool canAdd = false,
        System.Action<Attribute, float> cbAddStat = null, System.Action<Attribute, float> cbSubtractStat = null,
        System.Action<Attribute, RectTransform> cbSelected = null, System.Action cbDeselect = null,
        System.Action<BaseEventData> cbScroll = null)
    {
        if (attribute != dataLink)
        {
            dataLink = attribute;

            if (dataLink != null)
            {
                SetupName();

                //Setup Events
                OnAddStat = cbAddStat;
                OnSubtractStat = cbSubtractStat;
                OnSelected = cbSelected;
                OnDeselected = cbDeselect;
                OnScrolled = cbScroll;
            }
            else
            {
                //Clear Events
                OnAddStat = null;
                OnSubtractStat = null;
                OnSelected = null;
                OnDeselected = null;
                OnScrolled = null;
            }

            Refresh(canAdd);
        }
    }
    public Selectable getSelectable()
    {
        return selectable;
    }

    //Upkeep Methods
    public void Refresh(bool canAdd)
    {
        if (dataLink != null)
        {
            moddedValue.gameObject.SetActive(dataLink.valueBaseTemp != dataLink.valueModded);

            if (showBase)
                SetupValue(dataLink.valueBaseTemp, dataLink.valueModded);
            else
                SetupValue(dataLink.valueModded, dataLink.valueBaseTemp);

            //Only allow adding if there are free points available
            addButton.interactable = canAdd;

            //Only allow subtracting if there are temp points assigned
            subtractButton.interactable = (dataLink.valueTemp > 0);
        }
    }
    public void ShowButtons(bool show)
    {
        buttonsPanel.gameObject.SetActive(show);
    }
    void Add(float amount = 1f)
    {
        //Fire event
        if (OnAddStat != null)
            OnAddStat(dataLink, amount);
    }
    void Subtract(float amount = -1f)
    {
        //Fire event
        if (OnSubtractStat != null)
            OnSubtractStat(dataLink, amount);
    }

    //Event Handlers
    public void OnAdd()
    {
        Add();
    }
    public void OnSubtract()
    {
        Subtract();
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (OnSelected != null)
            OnSelected(dataLink, myTrans);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (OnDeselected != null)
            OnDeselected();
    }
    public void OnScroll(PointerEventData eventData)
    {
        if (OnScrolled != null)
            OnScrolled(eventData);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        OnAdd();
    }
    public void OnCancel(BaseEventData eventData)
    {
        OnSubtract();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        selectable.Select();
    }
}