using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class InputReturnEvent : UnityEvent<string>
{ }
public class GUI_InputBox : MonoBehaviour 
{
    public Text titleText;
    public InputField inputField;

    InputReturnEvent okayEvent = new InputReturnEvent();
    UnityEvent cancelEvent = new UnityEvent();

    IEnumerator closeLater()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }
    void closeOut()
    {
        okayEvent.RemoveAllListeners();
        cancelEvent.RemoveAllListeners();

        StartCoroutine(closeLater());
    }
    public void OnOK()
    {
        if (inputField.text != string.Empty)
        {
            okayEvent.Invoke(inputField.text);
            closeOut();
        }
        else
            OnCancel();
    }
    public void OnCancel()
    {
        cancelEvent.Invoke();
        closeOut();
    }
    public void setup(string title, UnityAction<string> okayAction, UnityAction cancelAction = null)
    {
        gameObject.SetActive(true);

        titleText.text = title;
        inputField.text = string.Empty;
        inputField.Select();

        okayEvent.AddListener(okayAction);
        if(cancelAction != null)
            cancelEvent.AddListener(cancelAction);
    }
}
