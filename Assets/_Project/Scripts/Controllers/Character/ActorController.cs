using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

/// <summary>
/// Base class for all controllers
/// NOTE: Should be never be added directly to a GameObject!  Use AIController or PlayerController instead!
/// </summary>
public class ActorController : MonoBehaviour
{
    public Light2D selfLight;

    public Actor myActor { get; private set; }
    protected Transform myTrans { get; private set; }

    protected virtual void Awake()
    {
        myTrans = GetComponent<Transform>();
    }
    protected virtual void Start()
    {
        SetControlTarget(GetComponent<Actor>());
        Initialize();
    }

    protected virtual void SubscribeToTargetEvents()
    {
        //myActor.AfterInitialized += Initialize;
        myActor.OnDied += Died;
        myActor.OnHit += Hit;
        myActor.weapon.OnWeaponChanged += WeaponChanged;
        myActor.OnChangeRoom += RoomChanged;
        myActor.OnGotItems += GotItems;
        myActor.OnInteractableObjectProximityChange += InteractableObjectProximityChange;
        myActor.OnGenerateInventory += GenerateInventory;
        myActor.OnNearestNPCChanged += NearestNPCChanged;
    }
    protected virtual void UncubscribeFromTargetEvents()
    {
        //myActor.AfterInitialized += Initialize;
        myActor.OnDied -= Died;
        myActor.OnHit -= Hit;
        myActor.weapon.OnWeaponChanged -= WeaponChanged;
        myActor.OnChangeRoom -= RoomChanged;
        myActor.OnGotItems -= GotItems;
        myActor.OnInteractableObjectProximityChange -= InteractableObjectProximityChange;
        myActor.OnGenerateInventory -= GenerateInventory;
    }
    protected virtual void SubscribeToTargetStatEvents()
    {
        myActor.characterSheet.HP.OnCurrentValueChanged += HPChanged;
    }
    protected virtual void UncubscribeFromTargetStatEvents()
    {
        myActor.characterSheet.HP.OnCurrentValueChanged -= HPChanged;
    }
    protected virtual void SetControlTarget(Actor target)
    {
        //If switching from an active control target, unsubscribe from that target's event
        if (myActor != null)
        {
            UncubscribeFromTargetStatEvents();
            UncubscribeFromTargetEvents();
        }

        //Change target
        myActor = target;

        //Subscribe to new target's events
        SubscribeToTargetEvents();
        SubscribeToTargetStatEvents();
    }

    //Actor event callback stubs
    protected virtual Actor.ActionResult ActivateAbility(BaseAbility ability)
    {
        //Nullcheck
        if (ability == null)
        {
            Debug.LogWarning("Attempted to use a null ability");
            return Actor.ActionResult.NotValid;
        }

        //Activate ability on my actor
        return myActor.ActivateAbility(ability, true, true, true);
    }
    protected virtual void Initialize() { }
    protected virtual void Died(Actor actor) { }
    protected virtual void Hit(Actor actor, HitData hit) { }
    protected virtual void WeaponChanged(BaseItem weapon) { }
    protected virtual void RoomChanged(Actor actor, Room newRoom) { }
    protected virtual void GotItems(Actor actor, string itemList) { }
    protected virtual void InteractableObjectProximityChange(Actor actor, InteractableObject iObject, bool entered) { }
    //Character sheet event callback stubs
    protected virtual void HPChanged(float newValue, float delta, float overflow) { }
    protected virtual void GenerateInventory() { }
    protected virtual void NearestNPCChanged(NPCController npc) { }

    //Public event calls
    public virtual void BarterEnd() { }
}
