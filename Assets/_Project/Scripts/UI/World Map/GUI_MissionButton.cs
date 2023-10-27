using UnityEngine;
using UnityEngine.UI;
using System;

public class GUI_MissionButton : MonoBehaviour 
{
    public RectTransform newThingy;
    public Text label;

    int referenceID = 0;

    public bool selected
    {
        set
        {
            if (button && value)
                button.Select();
        }
    }

    Action<int> OnClicked;
    Button button = null;

    void setNew(bool isNew)
    {
        if (newThingy)
            newThingy.gameObject.SetActive(isNew);
    }

    public void hide()
    {
        //Disable me
        gameObject.SetActive(false);

        //Clear event listeners
        OnClicked = null;
    }
    public void show(Action<int> OnClickedCallback, int referenceID, string labelText, bool newMission = false)
    {
        //Fetch component if needed
        if (button == null) button = GetComponent<Button>();

        //Subscribe to event
        OnClicked += OnClickedCallback;

        //Store referenceID
        this.referenceID = referenceID;

        //Set button label
        if (label) label.text = labelText;

        //Enable me
        gameObject.SetActive(true);

        //Show/Hide extra widgets
        setNew(newMission);
    }

    public void OnSubmit()
    {
        //Fire event
        if (OnClicked != null)
            OnClicked(referenceID);
    }
}