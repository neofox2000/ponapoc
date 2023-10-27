using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Alerter : MonoBehaviour 
{
    public float timeToShow = 3f;
    public static Alerter instance;
    public Text text;

    List<string> messages = new List<string>(); 
    Animator animator;
    
    static int akhShowing = Animator.StringToHash("Showing");
    static float showTimer = 0;

	void Awake() 
    {
        animator = GetComponent<Animator>();
        instance = this;
    }
    void Update()
    {
        if (showTimer > 0)
            showTimer -= Time.unscaledDeltaTime;
        else
        {
            if (animator.GetBool(akhShowing))
                animator.SetBool(akhShowing, false);
            else
                if(messages.Count > 0)
                    Pop();
        }
    }
    void Pop()
    {
        text.text = messages[0];
        GUI_Log.Log(text.text, true);
        messages.RemoveAt(0);
        showTimer = timeToShow;
        animator.SetBool(akhShowing, true);
    }
    public static void ShowMessage(string msg, float timeToShow, bool overrideQueue = true) 
    {
        instance.timeToShow = timeToShow;
        instance.messages.Add(msg);

        if (overrideQueue)
            showTimer = -1;
    }
    public static void ShowMessage(string msg, bool overrideQueue = true)
    {
        ShowMessage(msg, instance.timeToShow, overrideQueue);
    }
}
