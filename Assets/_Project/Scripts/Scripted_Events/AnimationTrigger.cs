using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public string animKey;
    public string activatorTag;
    public bool triggerOnEnter = false;
    public bool triggerOnExit = false;

    int akhAnimKey;

    void Awake()
    {
        akhAnimKey = Animator.StringToHash(animKey);
    }

    public void OnTriggered()
    {
        animator.SetTrigger(akhAnimKey);
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (triggerOnEnter && (collider.tag == activatorTag))
            OnTriggered();
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (triggerOnExit && (collider.tag == activatorTag))
            OnTriggered();
    }
}
