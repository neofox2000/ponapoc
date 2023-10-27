using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD_FloatingText : HUD_FloatingObject
{
    public Text label;
    public float timeToLive = 2f;
    public Vector2 tweenDistance = new Vector2(0, 50f);
    public string fireAnimKey = "Fire";

    int akFire;
    Animator animator;

    void OnEnable()
    {
        //Animator will not (and does not need to) be present
        //... when first spawned/enabled
        if (animator)
        {
            //Play animation after object is shown
            animator.SetTrigger(akFire);
        }
    }
    /// <summary>
    /// Fires from Animator.  Switches off object when finished.
    /// </summary>
    public void fireAnimEnd()
    {
        gameObject.SetActive(false);
    }
    public void fire(string text, Color textColor, Transform target)
    {
        //Cache stuff
        animator = GetComponent<Animator>();
        akFire = Animator.StringToHash(fireAnimKey);

        //Setup stuff
        animator.speed = 1 / timeToLive;
        label.text = text;
        label.color = textColor;

        //Call base object's fire method
        fire(target);
    }
    public void fire(string text, Color textColor, Vector3 fixedPosition)
    {
        targetPos = fixedPosition;
        fire(text, textColor, null);
    }
}
