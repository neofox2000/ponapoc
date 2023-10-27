using UnityEngine;

public class GUI_Loading : MonoBehaviour 
{
    Animator animator;
    int akhState = Animator.StringToHash("State");
    float deactivationTimer = 0;

    bool _doDeactivation = false;
    bool doDeactivation
    {
        get { return _doDeactivation; }
        set
        {
            _doDeactivation = value;
            if (value)
                deactivationTimer = 1;
        }
    }

    void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }
    void Update()
    {
        if(deactivationTimer > 0)
        {
            deactivationTimer -= Time.deltaTime;
        }
        else
        {
            if (doDeactivation)
                doDeactivation = false;
        }
    }
    
    void loadingComplete()
    {
        Awake();
        animator.SetInteger(akhState, 0);
    }
    void loadingStart()
    {
        Awake();
        animator.SetInteger(akhState, 1);
    }

    public void activate(bool slideIn = true)
    {
        loadingStart();
    }
    public void deactivate()
    {
        loadingComplete();
        doDeactivation = true;
    }
}