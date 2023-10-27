using UnityEngine;
using System.Collections;

public class GUI_ShowHideObject : MonoBehaviour 
{
    bool
        activeObject = false,
        _showing = false;

    Animator animator;

    public bool showing
    {
        get { return _showing; }
    }

	protected virtual void Awake() 
    {
        animator = GetComponent<Animator>();
	}
    protected void init()
    {
        if (!activeObject)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            activeObject = true;
        }
    }
    virtual public void ShowHide(bool showIt = true)
    {
        if (showIt)
            gameObject.SetActive(true);

        if (gameObject.activeSelf)
            Common.showStandardAnimatorObject(animator, showIt);
    }
    public void hideComplete()
    {
        _showing = false;
        //gameObject.SetActive(false);
    }
    public void showComplete()
    {
        _showing = true;
    }
}