using UnityEngine;
using System.Collections;

public class HUD_ColorFlash : MonoBehaviour 
{
    int akhPain = Animator.StringToHash("Pain");
    int akhLowLife = Animator.StringToHash("LowLife");

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void doPainFlash()
    {
        animator.SetBool(akhPain, true);
    }
    public void doPainFlashStarted()
    {
        animator.SetBool(akhPain, false);
    }
    public void setPainLowLife(bool lowLife)
    {
        animator.SetBool(akhLowLife, lowLife);
    }
}
