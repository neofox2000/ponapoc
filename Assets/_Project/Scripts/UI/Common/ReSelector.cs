using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ReSelector : MonoBehaviour
{
    StandaloneInputModule inputModule;
    Selectable lastSelected = null;

    bool canRun()
    {
        if (EventSystem.current)
        {
            if (!inputModule)
                inputModule = EventSystem.current.GetComponent<StandaloneInputModule>();

            return inputModule != null;
        }

        return false;
    }
    bool isSelectionInvalid(Selectable selectable)
    {
        return (selectable == null || !selectable.IsActive() || !selectable.IsInteractable());
    }
    /// <summary>
    /// Sifts through the array for a valid selection and selects it
    /// </summary>
    /// <param name="selectables"></param>
    /// <returns>true if a valid selection was made</returns>
    bool selectBestObject(Selectable[] selectables)
    {
        foreach (Selectable sel in selectables)
        {
            if (sel.IsActive() && 
               (sel.IsInteractable()) && 
               (sel.navigation.mode != Navigation.Mode.None))
            {
                sel.Select();
                return true;
            }
        }

        return false;
    }
    void selectRelevant(bool cancelButtonPressed = false)
    {
        bool nailedIt = false;

        //See if lastSelected object is a valid selection
        if(!isSelectionInvalid(lastSelected))
        {
            Selectable sel = lastSelected.GetComponent<Selectable>();
            if(sel.IsInteractable())
            {
                if(cancelButtonPressed)
                    sel.SendMessage(
                        "OnCancel", 
                        new BaseEventData(EventSystem.current), 
                        SendMessageOptions.DontRequireReceiver);
                else
                    sel.Select();

                nailedIt = true;
            }
        }

        //Look for the next valid selection
        if(!nailedIt)
        {
            Selectable[] selectables = null;
            if (lastSelected != null)
            {
                //Fetch parent selectables
                selectables = lastSelected.GetComponentsInParent<Selectable>(false);

                //Sift through the components for a valid selection
                nailedIt = selectBestObject(selectables);
            }


            //Still having found a valid selection?  Time to catch em all!
            selectables = FindObjectsOfType<Selectable>();

            //Sift through the components for a valid selection
            nailedIt = selectBestObject(selectables);
        }

        //Something might be wrong
        //if (!nailedIt)
            //Debug.LogWarning("No valid UI Navigation Selections found in entire scene!");
    }

	void Update ()
    {
        if (canRun())
        {
            GameObject currentSelectedGO = EventSystem.current.currentSelectedGameObject;
            Selectable currentSelectable = null;
            bool cancelButtonPressed = Input.GetButtonDown(inputModule.cancelButton);

            if (currentSelectedGO != null)
            {
                //Update lastSelected object
                if ((lastSelected == null) || (currentSelectedGO != lastSelected.gameObject))
                    lastSelected = currentSelectedGO.GetComponent<Selectable>();
            }

            //Respond to navigation input
            if ((Mathf.Abs(Input.GetAxis(inputModule.horizontalAxis)) > 0.25f) ||
                (Mathf.Abs(Input.GetAxis(inputModule.verticalAxis)) > 0.25f) ||
                 cancelButtonPressed)
            {
                if(currentSelectedGO != null)
                    currentSelectable = currentSelectedGO.GetComponent<Selectable>();

                //Only act if the current selection is invalid
                if (isSelectionInvalid(currentSelectable))
                {
                    //Listen for navigation events if no object is currently selected
                    selectRelevant(cancelButtonPressed);
                }
            }
        }
    }
}