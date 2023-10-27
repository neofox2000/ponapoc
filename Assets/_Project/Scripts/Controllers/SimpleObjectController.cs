using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(TDCharacterController2D))]
public class SimpleObjectController : ActorController
{
    #region Inspector Properties
    [Header("Core")]
    public float despawnDelay = 2.5f;
    public Transform colliders;

    [Header("Common Animation")]
    public string destroyTrigger = "Destroy";
    public string destroyState = "Destroy";
    #endregion

    //Events
    public Action OnOnlineStateChanged;

    //Private Properties
    //On/Off switch for Update handling
    bool _online = false;
    protected virtual bool online
    {
        get { return _online; }
        set
        {
            _online = value;

            //Fire event
            if (OnOnlineStateChanged != null)
                OnOnlineStateChanged();
        }
    }
    bool despawnOnDestroy = false;

    //Methods
    public virtual void init(int initalDirection, bool initialOnlineState, bool despawnOnDestroy)
    {
        //Switch back on collider if it was disabled
        ToggleColliders(true);

        this.despawnOnDestroy = despawnOnDestroy;
        online = initialOnlineState;
        myActor.dir = initalDirection;
    }

    protected IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(despawnDelay);

        Destroy(gameObject);
    }
    protected override void Died(Actor actor)
    {
        //Play destruction animation
        //animator.SetTrigger(destroyTrigger);

        //Switch off collider to prevent further hit checks
        ToggleColliders(false);

        //Turn off update handling
        online = false;

        //Call base method
        base.Died(actor);
    }
    void OnDestructionAnimationComplete()
    {
        //Expire animation completed - despawn back to pool.
        if (despawnOnDestroy)
            StartCoroutine(Cleanup());
    }
    void ToggleColliders(bool on)
    {
        colliders.gameObject.SetActive(on);
    }
}