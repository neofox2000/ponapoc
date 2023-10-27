using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GUI_Splashscreen : MonoBehaviour 
{
    bool done = true;
    Animator animator;

	void Awake() 
    {
        animator = GetComponent<Animator>();
	}
	void Update () 
    {
        if ((!done) && (Input.anyKeyDown))
            doMiddle();
	}

    public void doStartup()
    {

    }
    public void doMiddle()
    {
        done = true;
        animator.SetTrigger(Animator.StringToHash("Done"));
    }
    public void doEnding()
    {
        LoadingManager.SwitchScene(LoadingManager.SceneSelection.title);
    }

    public void activate()
    {
        GUIManager.instance.guiCommon.setBackground(true, Color.black);
        gameObject.SetActive(true);
        done = false;
        animator.SetTrigger(Animator.StringToHash("Reset"));
    }
    public void deactivate()
    {
        gameObject.SetActive(false);
    }
}