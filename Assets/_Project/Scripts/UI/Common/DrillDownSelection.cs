using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class DrillDownSelection : MonoBehaviour,
    IPointerEnterHandler, ISubmitHandler
{
    //public Selectable firstSelection;
    public List<Selectable> selections;
    public bool drilled = false;

    public Selectable selectable { get; set; }

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }
    private void drillOutOthers()
    {
        DrillDownSelection[] ddSelections = FindObjectsOfType<DrillDownSelection>();
        foreach (DrillDownSelection dds in ddSelections)
            if ((dds != this) && dds.drilled)
                dds.OnDrillUp(false);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        //Pass other mouse click types up the chain
        if ((eventData is PointerEventData) && (((PointerEventData)eventData).button != PointerEventData.InputButton.Left))
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.submitHandler);
            return;
        }

        OnDrillDown();
    }
    /// <summary>
    /// Drills down to child selections
    /// </summary>
    /// <param name="selection">Selects this instead if not null</param>
    public void OnDrillDown(Selectable selection = null)
    {
        if (selections.Count > 0)
        {
            //Drill out of any other drilldowns
            drillOutOthers();

            //Looks like the objects were destroyed; don't bother doing anything
            if (selections[0] == null) return;

            //Turn on selections
            foreach (Selectable sel in selections)
            {
                sel.interactable = true;
            }

            if (selection == null)
            {
                //Get the current selectable
                Selectable cSel = null;
                if (EventSystem.current.currentSelectedGameObject != null)
                    cSel = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                //If the currently selected object is not part of our list, then select the first item in our list
                if ((cSel != null) && !selections.Contains(cSel))
                {
                    Canvas.ForceUpdateCanvases();
                    selections[0].Select();
                }
            }
            else
                selection.Select();

            //Turn off my selectable
            selectable.interactable = false;

            //Set flag
            drilled = true;
        }
    }
    public void OnDrillUp(bool selectSelf = true)
    {
        //Turn on my selectable
        selectable.interactable = true;

        //Should I select myself?
        if (selectSelf)
        {
            //Get the current selectable
            Selectable cSel = null;
            if (EventSystem.current.currentSelectedGameObject != null)
                cSel = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

            //If something else hasn't already been selected, then select myself
            if ((cSel == null) || selections.Contains(cSel))
            {
                //Select my selectable
                selectable.Select();
                //Debug.Log(name + " selected");
            }
        }

        //Turn off selections
        foreach (Selectable sel in selections)
            sel.interactable = false;

        //Set flag
        drilled = false;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log(this);
        OnDrillDown();
    }

    /// <summary>
    /// Wire up selections to navigate to each other
    /// </summary>
    /// <param name="loopAround">Should the top and bottom selections loop back to each other?</param>
    public void SetupSelections(bool loopAround = true, bool horMirrorsVert = false)
    {
        //Setup row navigation and interactable state
        for (int i = 0; i < selections.Count; i++)
        {
            //Cache navigation links
            Selectable up = (i == 0 ? selections[selections.Count - 1] : selections[i - 1]);
            Selectable down = (i == selections.Count - 1 ? selections[0] : selections[i + 1]);
            
            //Setup selectable
            SetupSelectable(
                selections[i],
                drilled,
                up,
                down,
                horMirrorsVert ? up : null,
                horMirrorsVert ? down : null);
        }
    }
    public void SetupSelectable(Selectable target, bool interactable, Selectable up, Selectable down, Selectable left = null, Selectable right = null)
    {
        //Set interactability
        target.interactable = interactable;

        //Update navigation
        Navigation nav = target.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = up;
        nav.selectOnDown = down;
        nav.selectOnLeft = left;
        nav.selectOnRight = right;
        target.navigation = nav;
    }
}