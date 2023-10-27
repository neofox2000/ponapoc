using UnityEngine;
using RPGData;
using System;
using Variables;

public class InteractableObject : MonoBehaviour 
{
    #region Structures and Constants
    public enum ShowTextCondition { Never, PlayerOnly, Always }
    public enum AccessState { Open, Locked }
    
    //const int lockedAndBlocked = 1;
    //const int blockedState = 2;
    const int lockedState = 3;
    const int openState = 4;
    const float lockPickingMinTime = 1f;
    const float lockPickingMaxTime = 5f;
    const float lockPickingDecayTime = 0.5f;
    #endregion

    #region Inspector Properties
    [Header("Interactions")]
    [Tooltip("The animator that plays state transitions (eg: locked to unlocked)")]
    [SerializeField] Animator objectStateAnimator;
    [Tooltip("Does this object automatically activate when touched?")]
    public bool automatic;
    [Tooltip("Text to show when player touches this object")]
    [TextArea(3, 10)] public string floatingTextMessage = "[E] Use";
    [Tooltip("Does this object show the Floating Text Message when touched?")]
    [SerializeField] ShowTextCondition showText = ShowTextCondition.PlayerOnly;
    [Tooltip("Distance from object's origin to start floating text")]
    [SerializeField] Vector3 floatingTextOffset;
    [Tooltip("Used for non-instant interactions (eg: lockpicking)")]
    [SerializeField] FloatingBarData progressBar;

    [Header("Access Methods")]
    [Tooltip("The initial access state")]
    [SerializeField] AccessState accessState = AccessState.Open;
    [Tooltip("The key that will unlock this door")]
    [SerializeField] ItemTemplate keyItem;
    [Tooltip("Is the key destroyed when this door is unlocked?")]
    [SerializeField] bool keyConsumed = false;
    [Tooltip("Can lockpicking be used to open this?")]
    [SerializeField] bool canLockpick = false;
    [Tooltip("Skill required to pick lock")]
    [SerializeField] AttributeTemplate skillReq;
    [Tooltip("Minimum skill value required to pick lock")]
    [Range(1, 100)] [SerializeField] int minSkillRequired = 10;
    [Tooltip("Text to show when player fails to unlock this door")]
    [SerializeField] string lockedMessage = "It's locked";
    [Tooltip("Text to show when player successfully unlocks this door")]
    [SerializeField] string unlockedMessage = "Unlocked";

    [Header("Sounds")]
    [Tooltip("Sound played when this object is used")]
    [SerializeField] AudioGroupTemplate activationSound;
    [Tooltip("Sound played when this object cannot be used (locked/blocked/etc)")]
    [SerializeField] AudioGroupTemplate blockedSound;
    [Tooltip("Sound played when this object is successfully unlocked")]
    [SerializeField] AudioGroupTemplate unlockedSound;
    [Tooltip("Sound played when lockpicking is successful")]
    [SerializeField] AudioGroupTemplate lockpickSuccessSound;
    [Tooltip("Sound played when lockpicking is not possible (due to skill req or otherwise)")]
    [SerializeField] AudioGroupTemplate lockpickFailSound;
    [Tooltip("Sound played when lockpicking is cancelled")]
    [SerializeField] AudioGroupTemplate lockpickCancelSound;
    #endregion

    #region Events
    public Action OnLockpickingStarted;
    public Action OnLockpickingEnded;
    public Action<Actor> OnInteractAccessible;
    public Action<Actor> OnInteractInaccessible;
    #endregion

    #region Private Properties
    bool locked = false;
    bool lockPickingInProgress = false;
    float requiredTime = 999;
    float lockPickingProgress = 0;
    PersistentObject persistentObject;
    Actor interactor = null;
    #endregion

    #region Accessors
    float _lockPickingSkillLevelBeingUsed = 0f;
    float lockPickingSkillLevelBeingUsed
    {
        get { return _lockPickingSkillLevelBeingUsed; }
        set
        {
            _lockPickingSkillLevelBeingUsed = value;

            if (value > 0)
            {
                //Cache calculated time requirement
                requiredTime = Mathf.Clamp(
                    //Higher skill levels reduce lockpicking time
                    lockPickingMaxTime * (minSkillRequired / value),
                    //Keep time within min/max bounds
                    lockPickingMinTime,
                    lockPickingMaxTime);
            }
        }
    }
    #endregion

    //Mono Methods
    void Start()
    {
        //Cache component
        persistentObject = GetComponent<PersistentObject>();

        //Fetch persistent data to determine accessibility
        if ((persistentObject) && (persistentObject.GetState() != 0))
        {
            locked = persistentObject.GetState() == lockedState;
            //blocked = persistentObject.getState() == blockedState;

            //TODO: Add animation states

            return;
        }

        //Set initial accessType
        locked = accessState == AccessState.Locked;
    }
    void Update()
    {
        if (locked)
        {
            HandleLockPickingTimer();
            HandleLockPickingProgressVisual();
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        //Mark the floating text point
        Gizmos.DrawWireCube(
            transform.position + floatingTextOffset,
            Vector3.one * 0.5f);
    }

    //Generic Methods
    void HandleLockPickingTimer()
    {
        if (lockPickingInProgress)
        {
            //Increment progress
            lockPickingProgress += Time.deltaTime;

            //Check for completion
            if (lockPickingProgress >= requiredTime)
                LockPickSucceeded();
        }
        else
        {
            //Progress decays at half timestep
            if (lockPickingProgress > 0f)
                lockPickingProgress = Mathf.Max(
                    lockPickingProgress - (Time.deltaTime * 0.5f),
                    0f);
        }
    }
    void HandleLockPickingProgressVisual()
    {
        //Make sure we're good to go
        if (lockPickingProgress <= 0f) return;
        if (progressBar == null) return;

        //Update the current progress
        progressBar.currentProgress = lockPickingProgress;

        //Update display text
        progressBar.displayText = lockPickingInProgress ? "Working" : "Failing";
    }
    void TryUnlock(Actor interactor)
    {
        //Check player's inventory for key
        Actor actor = interactor.GetComponent<Actor>();
        BaseItem key = actor.inventory.FindItem(keyItem);
        if (key != null)
        {
            //Use key if present
            if (keyConsumed) actor.inventory.RemoveItem(key);
            //Set accessible state
            locked = false;
            //Play unlock sound
            AudioManager.instance.Play(unlockedSound);
            //Show unlock message
            ShowMessage(unlockedMessage + " using " + key.template.name);

            //Set persistent state
            if (persistentObject)
                persistentObject.SetState(openState);
        }
        else
        {
            /*if (canLockpick)
                startLockpicking(interactor.GetComponent<BaseController>());
            else*/
            {
                //Play locked sound if not present
                AudioManager.instance.Play(blockedSound);
                //Show locked message
                ShowMessage(lockedMessage);
            }
        }
    }
    void LockPickStart()
    {
        //Flag lock as being picked
        lockPickingInProgress = true;

        if (progressBar != null)
        {
            //Setup progress bar
            progressBar.target = transform;
            progressBar.offset = floatingTextOffset;
            progressBar.fillColor = Color.yellow;
            progressBar.showValueText = true;
            progressBar.showMaxValue = true;
            progressBar.maxProgress = requiredTime;
            progressBar.SetUsing(true);
        }

        //Fire Event
        if (OnLockpickingStarted != null) OnLockpickingStarted();
    }
    void LockPickEnd()
    {
        //Toggle lockpicking flag
        lockPickingInProgress = false;

        //Hide progress bar
        if (progressBar != null)
            progressBar.SetUsing(false);

        //Fire Event
        if (OnLockpickingEnded != null) OnLockpickingEnded();
    }
    void LockPickSucceeded()
    {
        //Set accessible state
        locked = false;
        
        //Play sound
        AudioManager.instance.Play(lockpickSuccessSound);
        
        //Show message
        ShowMessage(unlockedMessage, Color.green);
        
        //Common code run for every kind of end-of-lockpicking state change
        LockPickEnd();
    }
    void LockPickImpossible(string message)
    {
        //Play sound
        AudioManager.instance.Play(lockpickFailSound);
        //Show message
        ShowMessage(message, Color.yellow);
        //Fire Event
        if (OnLockpickingEnded != null) OnLockpickingEnded();
    }
    void LockPickCancelled()
    {
        //Play sound
        AudioManager.instance.Play(lockpickCancelSound);

        //Common code run for every kind of end-of-lockpicking state change
        LockPickEnd();
    }
    void ShowMessage(string msg, Color? color = null)
    {
        GameDatabase.sGameSettings.FloatSomeText(
            msg,
            color ?? Color.white,
            transform.position + floatingTextOffset);
    }
    void ShowFloatingTextMessage()
    {
        if (showText == ShowTextCondition.Never) return;

        if (interactor.controlledByPlayer || (showText == ShowTextCondition.Always))
            ShowMessage(floatingTextMessage);
    }
    public void InteractTouch(Actor interactor)
    {
        this.interactor = interactor;

        ShowFloatingTextMessage();
    }
    /// <summary>
    /// Use button tapped
    /// </summary>
    /// <param name="interactor"></param>
    public void InteractQuick(Actor interactor)
    {
        this.interactor = interactor;

        //Try to unlock if not accessible
        if (locked)
        {
            TryUnlock(interactor);

            //Fire event
            if (OnInteractInaccessible != null)
                OnInteractInaccessible(interactor);
        }
        else
        {
            //Play Sound?
            AudioManager.instance.Play(activationSound);

            //Fire event
            if (OnInteractAccessible != null)
                OnInteractAccessible(interactor);
        }
    }
    /// <summary>
    /// Use button being held down
    /// </summary>
    /// <param name="interactor"></param>
    public bool InteractLongStart(Actor interactor)
    {
        this.interactor = interactor;

        if (locked && canLockpick)
        {
            Actor controller = interactor.GetComponent<Actor>();

            if (controller)
            {
                //Cache skill level
                lockPickingSkillLevelBeingUsed =
                    controller.characterSheet.GetSkill(skillReq).valueModded;

                //Check skill for minimum requirement
                if (minSkillRequired <= lockPickingSkillLevelBeingUsed)
                {
                    //Start picking the lock
                    LockPickStart();

                    //Return successful interaction
                    return true;
                }
                else
                {
                    //Notify interactor that lock can't be picked
                    LockPickImpossible("Requires at least " + minSkillRequired + " " + skillReq.name);

                    return false;
                }
            }
        }

        return false;
    }
    public void InteractLongStop(Actor interactor)
    {
        this.interactor = interactor;

        if (lockPickingInProgress)
        {
            //Flag lock as no longer being picked
            lockPickingInProgress = false;

            //If still locked then the operation must have been cancelled
            if (locked)
                LockPickCancelled();
        }
    }
}