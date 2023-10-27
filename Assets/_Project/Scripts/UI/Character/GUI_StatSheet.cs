using UnityEngine;
using System.Collections.Generic;
using RPGData;

public class GUI_StatSheet : GUI_Sheet
{
    //Inspector Properties
    [Header("Core Stats")]
    [SerializeField] GameObject coreStatRowPrefab;
    [SerializeField] RectTransform coreStatPanel;

    [Header("Secondary Stats")]
    [SerializeField] GameObject standardRowPrefab;
    [SerializeField] RectTransform mainStatPanel;
    [SerializeField] RectTransform extraStatPanel;

    //Private Properties
    List<GUI_StatRow> rows = null;
    GUI_LabelInfoPanel[] extraStatRows = null;
    GUI_LabelInfoPanel[] mainStatRows = null;
    GUI_LabelInfoPanel currentLabel;

    //Accessors
    public float pointsRemaining
    {
        get
        {
            if (characterSheetLink != null)
                return characterSheetLink.availableAttributePoints.valueBaseTemp;
            else
                return 0;
        }
    }
    public bool canAdd
    {
        get { return pointsRemaining > 0f; }
    }

    //Events
    public System.Action<Attribute> OnAttributeSelected;

    //Event Handlers
    public void ChangeData(Attribute attribute, float amount)
    {
        characterSheetLink.ChangeTempAttribute(attribute, amount);
    }
    void OnInventoryChanged(BaseItem item, bool removed)
    {
        OnChanged();
    }
    void OnRowSelected(Attribute attribute, RectTransform trans)
    {
        if ((attribute != null) && (trans))
        {
            //Show popup if gui is loaded
            if (GUI_Common.instance)
            {
                GUI_Common.instance.ShowDetailsPopup(
                    attribute.template.description,
                    trans,
                    TextAlignment.Left);
            }

            //Fire Event
            if (OnAttributeSelected != null)
                OnAttributeSelected(attribute);
        }

    }
    void OnRowDeselected()
    {
        if (GUI_Common.instance)
            GUI_Common.instance.HideDetailsPopup();
    }
    public void OnSaveChanges()
    {
        if (characterSheetLink != null)
        {
            characterSheetLink.SaveTempAttributes();
            UpdateEditMode(false);
        }
    }

    //Base overrides
    protected override void OnChanged()
    {
        base.OnChanged();
    }
    protected override void UpdateEditMode(bool newEditMode)
    {
        //Set mode
        if (characterSheetLink != null)
        {
            //Edit mode is based on having free stat points to assign
            base.UpdateEditMode((characterSheetLink.availableAttributePoints.valueBase > 0));
        }
        else
            base.UpdateEditMode(false);

        //Update rows
        if (rows != null)
            foreach (GUI_StatRow statRow in rows)
                statRow.ShowButtons(editMode);
    }
    protected override void AddEvents()
    {
        if (characterSheetLink != null)
        {
            if (inventoryLink != null)
            {
                inventoryLink.OnSingleItemChanged += OnInventoryChanged;
                inventoryLink.OnMultipleItemsChanged += OnChanged;

                characterSheetLink.OnStatsChanged += OnChanged;

                base.AddEvents();
            } else Debug.LogWarning("GUI_CharacterSheet Enabled / inventoryLink = null");
        } else Debug.LogWarning("GUI_CharacterSheet Enabled / characterSheetLink = null");
    }
    protected override void RemoveEvents()
    {
        if (characterSheetLink != null)
        {
            if (inventoryLink != null)
            {
                inventoryLink.OnSingleItemChanged -= OnInventoryChanged;
                inventoryLink.OnMultipleItemsChanged -= OnChanged;

                characterSheetLink.OnStatsChanged -= OnChanged;

                base.RemoveEvents();
            } else Debug.LogWarning("GUI_CharacterSheet Enabled / inventoryLink = null");
        } //else Debug.LogWarning("GUI_CharacterSheet Enabled / characterSheetLink = null");
    }
    protected override void Refresh()
    {
        base.Refresh();

        //Check to see if gui has been destroyed (usually when quitting the game)
        if (!mainStatPanel)
            return;

        //Do refresh operation
        GameObject GO;
        int i, rowCount = 0;
        if (characterSheetLink != null)
            rowCount = characterSheetLink.attributes.Count;

        //Setup stats if there are any available
        if (rowCount > 0)
        {
            //Init statRows cache if it has not already been done, and resize if more are needed
            if ((rows == null) || (rows.Count != rowCount))
            {
                //Create new list
                rows = new List<GUI_StatRow>(rowCount);

                //Create rows
                for (i = 0; i < rowCount; i++)
                {
                    GO = Instantiate(coreStatRowPrefab) as GameObject;
                    GO.transform.SetParent(coreStatPanel.transform);

                    //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                    RectTransform RT = GO.GetComponent<RectTransform>();
                    RT.localScale = Vector3.one;

                    GUI_StatRow row = GO.GetComponent<GUI_StatRow>();
                    row.Setup(
                        characterSheetLink.GetAttribute(i),
                        canAdd,
                        ChangeData,
                        ChangeData,
                        OnRowSelected,
                        OnRowDeselected,
                        Scroll);
                    rows.Add(row);
                }
            }
            else
            {
                //Refresh row data
                foreach (GUI_StatRow statRow in rows)
                    statRow.Refresh(canAdd);
            }
        }
        else
        {
            //Clear rows
            if (rows != null)
            {
                foreach (GUI_StatRow row in rows)
                {
                    row.Setup(null);
                    if (row.gameObject)
                        Destroy(row.gameObject);
                }

                rows.Clear();
                rows = null;
            }
        }

        //Setup Main Stats
        if ((mainStatRows == null) || (mainStatRows.Length == 0))
        {
            mainStatRows = new GUI_LabelInfoPanel[4];
            for (i = 0; i < mainStatRows.Length; i++)
            {
                GO = Instantiate(standardRowPrefab) as GameObject;
                GO.transform.SetParent(mainStatPanel.transform);

                //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                RectTransform RT = GO.GetComponent<RectTransform>();
                RT.localScale = Vector3.one;

                currentLabel = GO.GetComponent<GUI_LabelInfoPanel>();

                switch (i)
                {
                    case 0: currentLabel.label.text = "Level"; break;
                    case 1: currentLabel.label.text = "Experience"; break;
                    case 2: currentLabel.label.text = "HP/MP/SP"; break;
                    case 3: currentLabel.label.text = "Available Points"; break;
                }

                mainStatRows[i] = currentLabel;
            }
        }

        //Setup Extra Stats
        if ((extraStatRows == null) || (extraStatRows.Length == 0))
        {
            extraStatRows = new GUI_LabelInfoPanel[13];
            for (i = 0; i < extraStatRows.Length; i++)
            {
                GO = Instantiate(standardRowPrefab) as GameObject;
                GO.transform.SetParent(extraStatPanel.transform);

                //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                RectTransform RT = GO.GetComponent<RectTransform>();
                RT.localScale = Vector3.one;

                currentLabel = GO.GetComponent<GUI_LabelInfoPanel>();

                switch (i)
                {
                    //case 0: currentLabel.label.text = "Unarmed DMG"; break;
                    //case 1: currentLabel.label.text = "Unarmed Crit %"; break;
                    //case 2: currentLabel.label.text = "Weapon DMG"; break;
                    //case 3: currentLabel.label.text = "Weapon Crit %"; break;
                    case 0: currentLabel.label.text = "(Unused)"; break;
                    case 1: currentLabel.label.text = "(Unused)"; break;
                    case 2: currentLabel.label.text = "Damage"; break;
                    case 3: currentLabel.label.text = "Crit %"; break;
                    case 4: currentLabel.label.text = "Protection"; break;
                    case 5: currentLabel.label.text = "Movement Speed"; break;
                    case 6: currentLabel.label.text = "Carrying Capacity"; break;
                    case 7: currentLabel.label.text = "Skills Points per Level"; break;
                    case 8: currentLabel.label.text = "Haggle Attempts"; break;
                    case 9: currentLabel.label.text = "SP Use per minute"; break;
                    case 10: currentLabel.label.text = "Contamination"; break;
                }

                extraStatRows[i] = currentLabel;
            }
        }

        if (characterSheetLink != null)
        {
            //Update Main Attribute Values
            mainStatRows[0].value.text = characterSheetLink.level.ToString(Common.defaultIntFormat);
            mainStatRows[1].value.text = characterSheetLink.XP.ToString(Common.defaultIntFormat) + " / " + characterSheetLink.XPRequired.ToString(Common.defaultIntFormat);
            mainStatRows[2].value.text =
                characterSheetLink.HP.valueModded.ToString(Common.defaultIntFormat) + " / " +
                characterSheetLink.MP.valueModded.ToString(Common.defaultIntFormat) + " / " +
                characterSheetLink.SP.valueModded.ToString(Common.defaultIntFormat);
            mainStatRows[3].value.text = pointsRemaining.ToString(Common.defaultIntFormat);

            //Update Extra Attribute Values
            //extraStatRows[0].value.text = characterSheetLink.unarmedDamage.ToString(Common.defaultFloatFormat);
            //extraStatRows[1].value.text = characterSheetLink.unarmedCritChance.ToString(Common.defaultFloatFormat);

            float val;

            //Weapon Stats
            extraStatRows[2].value.text = characterSheetLink.damage.ToString();
            extraStatRows[3].value.text = characterSheetLink.critRate.ToString(Common.defaultFloatFormat);

            //Defense Stats
            extraStatRows[4].value.text = characterSheetLink.protection.ToString();

            //Other stats
            extraStatRows[5].value.text = characterSheetLink.speedBase.ToString(Common.defaultFloatFormat);
            extraStatRows[6].value.text = characterSheetLink.carryWeight.ToString(Common.defaultFloatFormat);
            extraStatRows[7].value.text = characterSheetLink.skillPointsPerLevel.ToString(Common.defaultIntFormat);
            extraStatRows[8].value.text = characterSheetLink.haggleAttempts.ToString(Common.defaultIntFormat);
            extraStatRows[9].value.text = ((-characterSheetLink.SP.regenRateFixed) * 60).ToString(Common.defaultFloatFormat);

            //Contamination Points
            if (characterSheetLink.CP.valueModded > 0)
                val = characterSheetLink.CP.valueCurrent / characterSheetLink.CP.valueModded;
            else val = 0f;
            extraStatRows[10].value.text = val.ToString(Common.defaultIntPercentFormat);

            //Update edit mode
            UpdateEditMode(false);
        }
    }
}