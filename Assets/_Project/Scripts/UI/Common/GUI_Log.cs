using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fungus;

public class GUI_Log : MonoBehaviour
{
    public static GUI_Log instance = null;

    [SerializeField] Text text;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] float autoHideTime = 2.5f;

    public SayDialogEx sayDialog;
    public MenuDialogEx menuDialog;

    PanelFader panelFader = null;

    void Awake()
    {
        panelFader = GetComponent<PanelFader>();

        instance = this;

        Clear();

        //Register events
        if (sayDialog) sayDialog.OnSay += SayEvent;
        if (menuDialog) menuDialog.OnOptionChosen += MenuOptionChosenEvent;
    }
    public static void Log(string str, bool showIt)
    {
        instance.LogStr(str, showIt);
    }
    public static void Show()
    {
        instance.panelFader.Show(0);
    }
    public static void Hide()
    {
        instance.panelFader.Hide();
    }
    public static void Toggle()
    {
        if(instance.panelFader.isShowing)
            Hide();
        else
            Show();
    }
    public void LogStr(string str, bool showIt)
    {
        //Do we need to seperate lines?
        string appender = ((text.text == string.Empty) || (text.text == "")) ? string.Empty : "\n\n";

        //Append str to current text
        text.text = string.Concat(text.text, appender, str);

        //Scroll to new line (bottom)
        StartCoroutine(ScrollToBottom());

        //Show the panel
        if (showIt) panelFader.Show(autoHideTime);
    }
    public void Clear()
    {
        text.text = string.Empty;
    }
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
    void SayEvent(string text, Fungus.Character character)
    {
        //Log the convo text
        string logText = text;
        if (character != null)
        {
            string hexColor = ColorTypeConverter.ToRGBAHex(character.NameColor);
            logText = string.Concat(
                "<color=", hexColor, ">",
                character.NameText, "</color>",
                "\n", logText);
        }

        Log(logText, false);
    }
    void MenuOptionChosenEvent(string text)
    {
        Log(string.Concat("<color=#BABABAFF>You</color>\n", text), false);
    }
}