using UnityEngine;

public class DeployableObjectController : SimpleObjectController
{
    #region Inspector Properties
    [Header("Deployable Animation")]
    //Animation Keys
    public string deployTrigger = "Deploy";
    public string shutdownTrigger = "Shutdown";

    //Animation States
    public string deployState = "Deploy";
    public string shutdownState = "Shutdown";
    #endregion

    //Private Properties
    //How long before expiring (0 = never)
    float 
        expireTime = 0,
        expireTimer = 0;

    //Methods
    public void Init(Actor owner, int startingDirection, float duration)
    {
        //this.owner = owner;

        //Copy owner's physics layer to self and all child objects
        Common.ChangeLayersRecursively(transform, owner.gameObject.layer);

        //Start offline so that deploy animation can finish
        //Despawn when destroyed
        base.init(startingDirection, false, true);

        expireTime = duration;
        expireTimer = duration;

        //animator.SetTrigger(deployTrigger);
    }

    void Expire()
    {
        //Prevent expire from being called multiple times
        expireTime = 0;

        online = false;

        //Start expire animation
        //animator.SetTrigger(shutdownTrigger);

        //TODO: Create a proper way to self-terminate
        myActor.killSwitch = true;
    }

    /// <summary>
    /// Called from Animator ONLY
    /// </summary>
    public void OnShutdownAnimationComplete()
    {
        //Expire animation completed - despawn back to pool.
        StartCoroutine(Cleanup());
    }
    /// <summary>
    /// Called from Animator ONLY
    /// </summary>
    public void OnDeployAnimationComplete()
    {
        online = true;
    }

    protected virtual void Update()
    {
        if (!online) return;

        if (expireTime > 0)
        {
            if (expireTimer > 0)
                expireTimer -= Time.deltaTime;
            else
            {
                Expire();
            }
        }
    }
}
