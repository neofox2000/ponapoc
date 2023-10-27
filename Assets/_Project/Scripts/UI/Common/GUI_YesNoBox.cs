using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class GUI_YesNoBox : MonoBehaviour 
{
    public Text heading;
    public Image modalImage;
    //public string animKey = "Showing";

    UnityEvent yesEvent = new UnityEvent();
    UnityEvent noEvent = new UnityEvent();
    UnityEvent cancelEvent = new UnityEvent();

    public static GUI_YesNoBox instance;

    void Awake()
    {
        instance = this;
        Hide();
    }

    void Hide()
    {
        yesEvent.RemoveAllListeners();
        noEvent.RemoveAllListeners();
        cancelEvent.RemoveAllListeners();

        gameObject.SetActive(false);
    }
    public void Show(string question, bool modal = false, UnityAction yesAction = null, UnityAction noAction = null, UnityAction cancelAction = null)
    {
        heading.text = question;
        modalImage.enabled = modal;
        gameObject.SetActive(true);

        if (yesAction != null) yesEvent.AddListener(yesAction);
        if (noAction != null) noEvent.AddListener(noAction);
        if (cancelAction != null) cancelEvent.AddListener(cancelAction);
    }
    public bool isShowing()
    {
        return gameObject.activeSelf;
    }

    public void OnYes()
    {
        yesEvent.Invoke();
        Hide();
    }
    public void OnNo()
    {
        noEvent.Invoke();
        Hide();
    }
    public void OnCancel()
    {
        cancelEvent.Invoke();
        Hide();
    }
}
