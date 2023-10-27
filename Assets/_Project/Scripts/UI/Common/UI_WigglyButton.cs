using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UI_WigglyButton : MonoBehaviour 
{
    public string
        animKeyNormal = "Normal",
        animKeyPressed = "Pressed",
        animKeyHighlighted = "Highlighted";

    int akhNormal, akhPressed, akhHighlighted;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        akhNormal = Animator.StringToHash(animKeyNormal);
        akhPressed = Animator.StringToHash(animKeyPressed);
        akhHighlighted = Animator.StringToHash(animKeyHighlighted);
    }

    public void onPointerDown()
    {
        animator.SetTrigger(akhPressed);
    }
    public void onPointerUp()
    {
        animator.SetTrigger(akhNormal);
    }
    public void onPointerEnter()
    {
        animator.SetTrigger(akhHighlighted);
        SendMessageUpwards("setMouseInUseState", true, SendMessageOptions.DontRequireReceiver);
    }
    public void onPointerExit()
    {
        animator.SetTrigger(akhNormal);
        SendMessageUpwards("setMouseInUseState", false, SendMessageOptions.DontRequireReceiver);
    }
}
