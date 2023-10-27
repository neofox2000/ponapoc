using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using System;
using RPGData;

public class GUI_SkillRow : MonoBehaviour,
    IScrollHandler, ISelectHandler, IDeselectHandler,
    ICancelHandler, ISubmitHandler, IPointerEnterHandler
{
    public Text skillName, baseValue, moddedValue;
    public RectTransform buttonsPanel;
    public Button addButton, subtractButton;
    public bool showBase = true;

    RectTransform myTrans;
    Attribute dataLink;
    Selectable selectable;

    //Events
    public System.Action<Attribute, RectTransform> OnSelected;
    public System.Action OnDeselected;
    public System.Action<Attribute, float> OnAddSkill;
    public System.Action<Attribute, float> OnSubtractSkill;
    public System.Action<BaseEventData> OnScrolled;

    //Initialization
    void Awake()
    {
        myTrans = GetComponent<RectTransform>();
        selectable = GetComponent<Selectable>();
    }
    void SetupName()
    {
        skillName.text = dataLink.template.GetDisplayName();
        skillName.color = Color.white;
    }
    void SetupValue(float value, float valueModded)
    {
        if (value == 0)
            baseValue.text = "-";
        else
            baseValue.text = value.ToString("0");

        if (valueModded == 0)
            moddedValue.text = string.Empty;
        else
            moddedValue.text = valueModded.ToString("(0)");
    }
    public void Setup(Attribute skill, bool canAdd = false,
        System.Action<Attribute, float> cbAddStat = null, System.Action<Attribute, float> cbSubtractStat = null,
        System.Action<Attribute, RectTransform> cbSelected = null, System.Action cbDeselect = null,
        System.Action<BaseEventData> cbScroll = null)
    {
        if (skill != dataLink)
        {
            dataLink = skill;

            if (dataLink != null)
            {
                SetupName();

                //Setup Events
                OnAddSkill = cbAddStat;
                OnSubtractSkill = cbSubtractStat;
                OnSelected = cbSelected;
                OnDeselected = cbDeselect;
                OnScrolled = cbScroll;
            }
            else
            {
                //Clear Events
                OnAddSkill = null;
                OnSubtractSkill = null;
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

    //Upkeep
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

    //Event Handlers
    public void OnAdd()
    {
        if (OnAddSkill != null)
        {
            if (Common.getShiftState())
            {
                //All remaining points
                OnAddSkill(dataLink, 0f);

                //if (manager != null)
                //changeData(manager.pointsRemaining);
            }
            else
            {
                if (Common.getControlState())
                    //changeData(+5f);
                    OnAddSkill(dataLink, +5f);
                else
                    //changeData(+1f);
                    OnAddSkill(dataLink, +1f);
            }
        }
    }
    public void OnSubtract()
    {
        if (OnSubtractSkill != null)
        {
            if (Common.getShiftState())
            {
                OnSubtractSkill(dataLink, 0);
            }
            else
            {
                if (Common.getControlState())
                    OnSubtractSkill(dataLink, -5f);
                else
                    OnSubtractSkill(dataLink, -1f);
            }
        }
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
        //if(manager != null) manager.Scroll(eventData);
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