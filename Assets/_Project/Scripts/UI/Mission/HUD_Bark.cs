using UnityEngine;
using UnityEngine.UI;

public class HUD_Bark : HUD_FloatingObject 
{
    const float timePerCharacter = 0.05f;
    const float minDuration = 2.5f;

    //public CanvasGroup canvasGroup;
    [SerializeField] protected Animator animator;
    [SerializeField] protected string animationKey = "Play";
    [SerializeField] protected Text labelBark, labelName;

    public System.Action OnBarkComplete;

    float timer = 0;
    public float timeLeft
    {
        get { return timer; }
    }

    protected override void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                //End bark animation
                if (animator)
                    animator.SetBool(Animator.StringToHash(animationKey), false);
                else
                    barkComplete();
            }
        }

        base.Update();
    }
    public void barkComplete()
    {
        //Notify target that bark is finished
        if (OnBarkComplete != null)
            OnBarkComplete();

        gameObject.SetActive(false);
    }
    public void fire(Transform target, string text)
    {
        //Put text into GUI
        labelBark.text = text;

        //Determine how long to remain visible
        timer = text.Length * timePerCharacter;
        if (timer < minDuration)
            timer = minDuration;

        //Run base code
        fire(target);

        //Start bark animation
        if (animator)
            animator.SetBool(Animator.StringToHash(animationKey), true);
    }
}
