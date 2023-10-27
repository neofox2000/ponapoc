using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
//using System;
using RPGData;

public class GUI_SkillSheet : GUI_Sheet 
{
    //Inspector Properties
    [SerializeField] Text availablePoints;
    [SerializeField] GameObject rowPrefab;

    //Private Properties
    bool skipSort = false;
    List<GUI_SkillRow> rows = null;

    //Accessors
    public float pointsRemaining
    {
        get
        {
            if (characterSheetLink != null)
                return characterSheetLink.availableSkillPoints.valueBaseTemp;
            else
                return 0;
        }
    }
    public bool canAdd
    {
        get { return pointsRemaining > 0f; }
    }

    //Events
    public System.Action<Attribute> OnSkillSelected;

    //Event Handlers
    public void changeData(Attribute skill, float amount)
    {
        skipSort = true;
        characterSheetLink.ChangeTempSkill(
            skill, 
            amount == 0 ? pointsRemaining : amount);
    }
    void OnRowSelected(Attribute skill, RectTransform trans)
    {
        if ((skill != null) && (trans))
        {
            if (GUI_Common.instance)
            {
                TextAlignment textAlign =
                    GUI_Common.instance.characterSheetGUI.mode == GUI_CharacterSheet.CharacterSheetModes.Creation ?
                    TextAlignment.Left :
                    TextAlignment.Right;

                GUI_Common.instance.ShowDetailsPopup(
                    skill.template.description,
                    trans,
                    textAlign);
            }

            //Fire Event
            if (OnSkillSelected != null)
                OnSkillSelected(skill);
        }
    }
    void OnRowDeselected()
    {
        if (GUI_Common.instance)
            GUI_Common.instance.HideDetailsPopup();
    }
    void OnRowSubmitted()
    {

    }
    void OnRowCancelled()
    {
        //GUI_Common.instance.OnMenuButtonPressed();
        //drillDownSelection.OnDrillUp();
    }
    public void OnSaveChanges()
    {
        if (characterSheetLink != null)
        {
            characterSheetLink.SaveTempSkills();
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
            base.UpdateEditMode((characterSheetLink.availableSkillPoints.valueBase > 0));
        }
        else
            base.UpdateEditMode(false);

        //Update rows
        if(rows != null)
            foreach (GUI_SkillRow row in rows)
                row.ShowButtons(editMode);
    }
    protected override void AddEvents()
    {
        if (characterSheetLink != null)
        {
            characterSheetLink.OnStatsChanged += OnChanged;
            base.AddEvents();
        }
        else Debug.LogWarning("GUI_SkillSheet Enabled / characterSheetLink = null");
    }
    protected override void RemoveEvents()
    {
        //if (eventsAreOn)
        {
            if (characterSheetLink != null)
            {
                if (characterSheetLink != null)
                    characterSheetLink.OnStatsChanged -= OnChanged;
                else Debug.LogWarning("GUI_SkillSheet Enabled / characterSheetLink = null");
            }
            //else Debug.LogWarning("GUI_SkillSheet Disabled / controllerLink = null");

            base.RemoveEvents();
        }
    }
    protected override void Refresh()
    {
        base.Refresh();

        GameObject GO;
        int i, rowCount = 0;
        if(characterSheetLink != null)
            rowCount = characterSheetLink.skills.Count;

        if (availablePoints)
            availablePoints.text = pointsRemaining.ToString(Common.defaultIntFormat);

        //Setup stats if there are any available
        if (rowCount > 0)
        {
            //Init statRows cache
            if ((rows == null) || (rows.Count != rowCount))
            {
                rows = new List<GUI_SkillRow>(rowCount);
                for (i = 0; i < rowCount; i++)
                {
                    GO = Instantiate(rowPrefab) as GameObject;
                    GO.transform.SetParent(scrollRect.content.transform);

                    //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                    RectTransform RT = GO.GetComponent<RectTransform>();
                    RT.localScale = Vector3.one;

                    GUI_SkillRow row = GO.GetComponent<GUI_SkillRow>();
                    row.Setup(characterSheetLink.GetSkillIndex(i),
                        canAdd,
                        changeData,
                        changeData,
                        OnRowSelected,
                        OnRowDeselected,
                        Scroll);
                    rows.Add(row);
                }
            }
            else
            {
                foreach (GUI_SkillRow row in rows)
                    row.Refresh(canAdd);
            }

            //Sort first (why?)
            if (!skipSort) characterSheetLink.SortSkills();

            skipSort = false;
        }
        else
        {
            //Clean up existing rows
            if (rows != null)
            {
                foreach (GUI_SkillRow row in rows)
                {
                    row.Setup(null);
                    if(row.gameObject != null)
                        Destroy(row.gameObject);
                }

                rows.Clear();
                rows = null;
            }
        }

        UpdateEditMode(false);
    }
}