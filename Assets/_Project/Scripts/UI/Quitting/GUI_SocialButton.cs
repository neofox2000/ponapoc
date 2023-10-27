using UnityEngine;
using System.Collections;

public class GUI_SocialButton : MonoBehaviour 
{
    public string hintAnimKey = "Hint";
    public string url = "";
    Animator animator;
    int akhHint;

	void Awake() 
    {
        animator = GetComponent<Animator>();
        akhHint = Animator.StringToHash("Hint");
    }

    void showHint(bool show)
    {
        animator.SetBool(akhHint, show);
    }

    #region Events
    public void OnHoverOver()
    {
        showHint(true);
    }
    public void OnHoverOut()
    {
        showHint(false);
    }
    public void OnClick()
    {
        Application.OpenURL(url);
    }
    #endregion
}