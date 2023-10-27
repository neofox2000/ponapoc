using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class BaseWeapon
{
    //Structures
    public enum WeaponState { Idle = 0, Firing = 1, Reloading = 2, ReloadEnd = 3, DryFiring = 4, Interrupted = 5, Cleanup = 99 }
    public enum WeaponSoundRef { fire = 0, empty = 1, reloadStart = 2, reloadMid = 3, reloadEnd = 4 }

    //Public properties
    public WeaponTemplate template = null;
    public SpineEquipmentManager.EquipmentSpineSlot slot = null;
    public BaseItem item
    {
        get
        {
            if (slot != null)
                return slot.equippedItem;

            return null;
        }
        set
        {
            //Change character visual
            if (slot != null)
                slot.equippedItem = value;

            //Fire event
            if (OnWeaponChanged != null)
                OnWeaponChanged(value);
        }
    }

    public int
        ammoPerReloadCycle = 0,
        ammoCountToReloadWith = 0;

    //Events
    public System.Action<BaseItem> OnWeaponChanged;

    //Accessors
    WeaponState _currentState = WeaponState.Idle;
    public WeaponState currentState
    {
        get
        {
            //return (WeaponState)character.animator.GetInteger(akhState);
            return _currentState;
        }
        set
        {
            if (currentState != value)
            {
                //Debug.Log("[" + Time.time + "] currentState: " + currentState + "\nrequrestedState: " + value);
                //animator.SetInteger(akhState, (int)value);
                _currentState = value;

                /*switch (value)
                {
                    case WeaponState.Firing:
                        if (firingAnim)
                        {
                            animator.speed = baseFireAnimationTime / itemLink.usedelay;
                            //Debug.Log("animator.speed: " + animator.speed + " = baseFireAnimationTime: " + baseFireAnimationTime + " / itemLink.usedelay: " + itemLink.usedelay);
                        }
                        else
                            animator.speed = 1;
                        break;

                    case WeaponState.Reloading:
                        if (reloadAnims.Length > 0)
                            animator.speed = baseReloadAnimationTime / itemLink.reloaddelay;
                        else
                            animator.speed = 1;
                        break;

                    case WeaponState.ReloadEnd:
                        break;

                    default:
                        animator.speed = 1;
                        break;
                }*/
            }
        }
    }

    //Private Properties
    SortingGroup parentSortingGroup;

    //Methods
    public BaseWeapon(SortingGroup parentSortingGroup)
    {
        this.parentSortingGroup = parentSortingGroup;
    }
    void SetupReloadMethod()
    {
        //Setup reload method variables
        if (item != null)
        {
            if (template.reloadMethod == WeaponTemplate.ReloadMethods.WholeClip)
                ammoPerReloadCycle = item.template.chargeCount;
            else
                ammoPerReloadCycle = 1;
        }
    }
    public void ChangeSlot(SpineEquipmentManager.EquipmentSpineSlot slot)
    {
        this.slot = slot;
    }
    public void Equip(BaseItem item, WeaponTemplate template)
    {
        this.item = item;
        this.template = template;
        SetupReloadMethod();
        currentState = WeaponState.Idle;
    }

    void PlayWeaponSoundDirect(Transform position, AudioGroupTemplate sound)
    {
        AudioManager.instance.Play(sound, position);
    }
    public void PlayWeaponSound(Transform position, WeaponSoundRef sound)
    {
        if (template != null)
        {
            AudioGroupTemplate soundToPlay = null;
            switch (sound)
            {
                case WeaponSoundRef.fire: soundToPlay = template.sounds.fire; break;
                case WeaponSoundRef.empty: soundToPlay = template.sounds.empty; break;
                case WeaponSoundRef.reloadStart: soundToPlay = template.sounds.reloadStart; break;
                case WeaponSoundRef.reloadMid: soundToPlay = template.sounds.reloadMid; break;
                case WeaponSoundRef.reloadEnd: soundToPlay = template.sounds.reloadEnd; break;
            }

            if(soundToPlay != null)
                PlayWeaponSoundDirect(position, soundToPlay);
        }
        else
            Debug.LogWarning("Attempting to playWeaponSound without a weapon - check animation events");
    }

    public void DoTracerEffects(Transform anchor, Vector3 start, Vector3 end, int direction)
    {
        TracerEffect tEffect;
        foreach (CommonStructures.EffectSpawnRelative effectData in template.tracerEffects)
        {
            //Spawn prefab
            tEffect = Common.SpawnEffect(effectData, anchor, direction, 0)
                .GetComponent<TracerEffect>();

            //Setup tracer points and draw order
            tEffect.fire(
                start,
                end,
                parentSortingGroup);
        }
    }
    public void DoFireEffects(Transform anchor, int direction)
    {
        foreach (CommonStructures.EffectSpawnRelative effect in template.fireEffects)
            Common.SpawnEffect(
                effect,
                anchor,
                direction,
                parentSortingGroup.sortingOrder + 1);
    }
    public void DoEjectionEffects(Transform anchor, int direction, float groundLevel)
    {
        BulletCasingPhysics casing;
        foreach (CommonStructures.EffectSpawnRelative effectData in template.ejectionEffects)
        {
            casing = Common.SpawnEffect(
                effectData,
                anchor,
                direction,
                parentSortingGroup.sortingOrder + 1)
                .GetComponent<BulletCasingPhysics>();

            if (casing) casing.doInit(direction, groundLevel);
        }
    }

    public bool canFire { get { return ( ((currentState == WeaponState.Idle) || (currentState == WeaponState.Reloading))); } }
    public bool canReload { get { return ((currentState == WeaponState.Idle)); } }
}
