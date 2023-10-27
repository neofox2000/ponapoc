using UnityEngine;
using UnityEngine.UI;
using RPGData;
using Variables;

public class GUI_CharacterSheet : MonoBehaviour
{
    //Enums
    public enum CharacterSheetModes { Normal = 0, Creation = 1 };
    public enum FocusWindow { Items, ItemQuickSlots, Skills, Stats, Abilities }

    //Inspector Properties
    [Header("Main")]
    public GUI_StatSheet statSheet;
    public GUI_SkillSheet skillSheet;
    public GUI_AbilitiesSheet abilitySheet;
    public GUI_InventoryManager inventorySheet;
    public GUI_CharacterCreation characterCreationPanel;
    public GUI_InventoryQuickSlotsManager quickslotManager;
    public UI_Pagecontrol LeftPanelTabControl;
    public UI_Pagecontrol RightPanelTabControl;
    public Text keyMouseHelp;
    public Text controllerHelp;

    [Header("Help Text")]
    public string statHelpKeyMouse;
    public string statHelpController;
    public string skillHelpKeyMouse;
    public string skillHelpController;
    public string itemHelpKeyMouse;
    public string itemHelpController;
    public string quickslotHelpKeyMouse;
    public string quickslotController;
    public string abilityHelpKeyMouse;
    public string abilityHelpController;

    //Private Properties
    bool rtDown = false;
    bool ltDown = false;
    RectTransform myTrans;

    //Accessors
    FocusWindow _currentFocusWindow = FocusWindow.Items;
    public FocusWindow currentFocusWindow
    {
        get { return _currentFocusWindow; }
        protected set
        {
            _currentFocusWindow = value;
            
            //Setup help text based on the new focus
            switch(value)
            {
                case FocusWindow.Items:
                    keyMouseHelp.text = itemHelpKeyMouse;
                    controllerHelp.text = itemHelpController;
                    break;
                case FocusWindow.Skills:
                    keyMouseHelp.text = skillHelpKeyMouse;
                    controllerHelp.text = skillHelpController;
                    break;
                case FocusWindow.Stats:
                    keyMouseHelp.text = statHelpKeyMouse;
                    controllerHelp.text = statHelpController;
                    break;
                case FocusWindow.Abilities:
                    keyMouseHelp.text = abilityHelpKeyMouse;
                    controllerHelp.text = abilityHelpController;
                    break;
                case FocusWindow.ItemQuickSlots:
                    keyMouseHelp.text = quickslotHelpKeyMouse;
                    controllerHelp.text = quickslotController;
                    break;
            }
        }
    }
    public CharacterSheetModes mode { get; protected set; }

    private void Awake()
    {
        inventorySheet.OnItemSelected += ItemSelected;
        statSheet.OnAttributeSelected += AttributeSelected;
        skillSheet.OnSkillSelected += SkillSelected;
        quickslotManager.OnQuickSlotSelected += ItemQuickSlotSelected;
    }
    void ItemSelected(UI_ListItem item)
    {
        currentFocusWindow = FocusWindow.Items;
    }
    void ItemQuickSlotSelected(GUI_InventoryQuickSlot slot)
    {
        currentFocusWindow = FocusWindow.ItemQuickSlots;
    }
    void AttributeSelected(Attribute attribute)
    {
        currentFocusWindow = FocusWindow.Stats;
    }
    void SkillSelected(Attribute skill)
    {
        currentFocusWindow = FocusWindow.Skills;
    }

    public void Show(CharacterSheetModes mode)
    {
        if (!myTrans) myTrans = GetComponent<RectTransform>();

        this.mode = mode;

        //Connect(target.Value.myActor);

        //Toggle relevant visual elements
        switch (mode)
        {
            case CharacterSheetModes.Normal:
                //Turn off character creation panel
                characterCreationPanel.gameObject.SetActive(false);
                //Hide inventory tab
                RightPanelTabControl.ShowHideTab(0, true);
                //Hide Abilities tab
                RightPanelTabControl.ShowHideTab(2, true);
                //TODO: Add ability choices to creation process
                break;
            case CharacterSheetModes.Creation:
                //Turn on character creation panel
                characterCreationPanel.gameObject.SetActive(true);
                //Show inventory tab
                RightPanelTabControl.ShowHideTab(0, false);
                //Show Abilities tab
                RightPanelTabControl.ShowHideTab(2, false);
                break;
        }
    }
    public void Hide()
    {
        abilitySheet.CancelSlotAssignment(false);
        quickslotManager.CancelSlotAssignmentProcess(false);
        statSheet.OnSaveChanges();
        skillSheet.OnSaveChanges();
    }

    void HandleGlobalInput()
    {
        float xboxTriggerValue = InputX.Axis(AxisCode.Joystick3);

        //Left Trigger Down?
        if (!ltDown && (xboxTriggerValue > 0.5f))
        {
            ltDown = true;
            GUI_Common.instance.HideSubPanels();
            LeftPanelTabControl.PrevTab();
        }
        //Left Trigger Up?
        if (ltDown && (xboxTriggerValue < 0.5f))
        {
            ltDown = false;
        }

        //Right Trigger Down?
        if (!rtDown && (xboxTriggerValue < -0.5f))
        {
            rtDown = true;
            GUI_Common.instance.HideSubPanels();
            LeftPanelTabControl.NextTab();
        }
        //Right Trigger Up?
        if (rtDown && (xboxTriggerValue > -0.5f))
        {
            rtDown = false;
        }

        //Bumpers (normal buttons)
        if (InputX.Down(InputCode.XboxLeftBumper))
        {
            GUI_Common.instance.HideSubPanels();
            RightPanelTabControl.PrevTab();
        }
        if (InputX.Down(InputCode.XboxRightBumper))
        {
            GUI_Common.instance.HideSubPanels();
            RightPanelTabControl.NextTab();
        }
    }
    private void OnEnable()
    {
        inventorySheet.inventoryLink = GameDatabase.lInventory;
        quickslotManager.LoadFromData();
    }
    private void OnDisable()
    {
        quickslotManager.ClearAll();
        inventorySheet.inventoryLink = null;
    }
    private void Update()
    {
        HandleGlobalInput();
    }
}