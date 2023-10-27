using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using SardonicMe.Perlib;

public class GUI_SavegameItem : MonoBehaviour, 
    IScrollHandler, ISubmitHandler, ICancelHandler, ISelectHandler,
    IPointerUpHandler
{
    //Inspector Properties
    //[SerializeField] Text saveName;
    [SerializeField] Text saveDetails;

    //Accessors
    public bool hasData { get; protected set; }
    public int saveIndex { get; set; }

    //Private Properties
    public Selectable selectable { get; protected set; }
    public RectTransform rTrans { get; protected set; }

    //Events
    public Action<GUI_SavegameItem> OnSelectAction;
    public Action OnCancelAction;
    public Action<GUI_SavegameItem> OnSubmitAction;
    public Action<PointerEventData> OnScrollAction;

    //Event Handlers
    public void OnCancel(BaseEventData eventData)
    {
        if (OnCancelAction != null)
            OnCancelAction();
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if (OnSubmitAction != null)
            OnSubmitAction(this);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        OnSubmit(eventData);
    }
    public void OnScroll(PointerEventData eventData)
    {
        if (OnScrollAction != null)
            OnScrollAction(eventData);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (OnSelectAction != null)
            OnSelectAction(this);
    }

    //Init Methods
    public void init(
        int saveIndex, 
        Action<GUI_SavegameItem> selectAction, Action cancelAction, 
        Action<GUI_SavegameItem> submitAction, Action<PointerEventData> scrollAction)
    {
        if (!rTrans) rTrans = GetComponent<RectTransform>();
        if (!selectable) selectable = GetComponent<Selectable>();

        this.saveIndex = saveIndex;
        OnSelectAction += selectAction;
        OnCancelAction += cancelAction;
        OnSubmitAction += submitAction;
        OnScrollAction += scrollAction;
    }
    public void setup(int slotNo)
    {
        hasData = false;
        //saveName.enabled = true;
        //saveName.text = "Slot " + (slotNo + 1).ToString("000");
        saveDetails.text = "Empty Slot";

        try
        {
            string fn = PlayerData.MakeSnapSaveFN(slotNo);
            Perlib saveSnap = new Perlib(fn);
            if (saveSnap.Exists)
            {
                saveSnap.Open();
                if (saveSnap.HasKey("saveVersion") && saveSnap.GetValue<int>("saveVersion") == PlayerData.currentSaveVersion)
                {
                    saveDetails.text = string.Concat(
                        "Level ", saveSnap.GetValue<float>("level"), "\n",
                        "XP ", saveSnap.GetValue<float>("XP") //.ToString(Common.defaultIntFormat)
                    );

                    hasData = true;
                }
                else
                    saveDetails.text = "INCOMPATIBLE";
            }
        }
        catch
        {
            saveDetails.text = "BAD SAVE!";
        }

    }
}