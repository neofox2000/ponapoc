using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Character : MonoBehaviour
{
    public enum Voices { idle = 0, seek = 1, attack = 2, die = 3, pain = 4, unarmed = 5, melee = 6, footstep = 7, unused1 = 8, unused2 = 9, dash = 10 }
    public enum Motions { Idle, Walk, Run, Limp }
    public enum AttackType { Unarmed, MeleeWeapon, RangedWeapon, ThrowingWeapon }

    #region Inspector Properties
    [Header("Debugging")]
    public bool test = false;

    [Header("Visuals")]
    public bool badDesignedRotation = false;
    public int staggerAnimations = 1;
    public Vector2 characterWidthBySide = new Vector2(1,1);
    public Vector2 controllerOffset, floatingObjectOffset;

    [Header("Spine Bone Emitters")]
    public Transform specialEffectsEmitter;
    public Transform[] meleeAttackEmitters;
    public Transform[] weaponAnchors;

    [Header("Audio")]
    public float voiceDelay = 1f;
    #endregion

    #region Private/Hidden Properties
    private bool awoken = false;
    protected float voiceTimer = 0f;

    //HitBox[] hitBoxes;
    protected Transform myTrans;
    protected Animator animator;
    protected SortingGroup sortingGroup;
    [HideInInspector] public SpineEquipmentManager spineEquipmentManager = null;
    #endregion

    #region Events
    public Action<int> OnAttackHitCheck;
    public Action<float> OnCameraShake;
    public Action<float> OnMoveCharacterX;
    public Action<float> OnMoveCharacterY;
    public Action<int> OnPlayWeaponSound;
    //public Action<float, bool> OnPlayCharacterSound;
    public Action<int> OnThrowingWeaponRelease;
    public Action<Transform> OnExecuteAbility;
    public Action OnReloadAmmoAdd;
    public Action<StateMachineParameters> OnStandardAnimatorStateEnter;
    public Action<StateMachineParameters> OnStandardAnimatorStateExit;
    //public Action<Transform, int> OnSpawnEffect;
    #endregion

    #region Accessors
    public CharacterTemplate template { get; set; }

    bool _dead = false;
    public bool dead
    {
        get { return _dead; }
        set
        {
            if (_dead != value)
            {
                _dead = value;
                if (_dead)
                {
                    int deadAnimSetting = UnityEngine.Random.Range(0, 2);
                    animator.SetFloat(akhDeadAnim, deadAnimSetting);
                }

                animator.SetBool(akhDead, _dead);
            }
        }
    }

    bool _sneaking = false;
    public bool sneaking
    {
        get { return _sneaking; }
        set
        {
            if (_sneaking != value)
            {
                _sneaking = value;
                animator.SetBool(akhSneaking, value);
            }
        }
    }

    bool _interacting = false;
    public bool interacting
    {
        get { return _interacting; }
        set
        {
            if (_interacting != value)
            {
                _interacting = value;
                animator.SetBool(akhInteracting, value);
            }
        }
    }

    public bool flying
    {
        get { return animator.GetBool(akhFlying); }
        set { animator.SetBool(akhFlying, value); }
    }

    //The direction that the parent controller is facing
    public int direction { get; set; }

    Motions _motion = Motions.Idle;
    public Motions motion
    {
        get { return _motion; }
        set
        {
            if (value != _motion)
            {
                _motion = value;

                //Moving state has changed.  Update animator and sound.
                animator.SetFloat(akhMotion, (float)_motion);

                /*
                //TODO: Needs change-over to footstep keyframe events
                if ((_motion != Motions.Idle) && (!sneaking))
                    playVoiceEnum(Voices.walk, false);
                else
                    playVoiceEnum(Voices.walk, true);
                */
            }
        }
    }

    int _weaponAnimationKey = 0;
    public int weaponAnimationKey
    {
        get { return _weaponAnimationKey; }
        set
        {
            _weaponAnimationKey = value;
            animator.SetInteger(akhWeaponClass, value);
        }
    }

    WeaponTemplate.ReloadMethods _reloadMethod = WeaponTemplate.ReloadMethods.WholeClip;
    public WeaponTemplate.ReloadMethods reloadMethod
    {
        get { return _reloadMethod; }
        set
        {
            _reloadMethod = value;
            animator.SetInteger(akhReloadMethod, (int)value);
        }
    }

    float _attackSpeed = 1;
    public float attackSpeed
    {
        get { return _attackSpeed; }
        set
        {
            if(_attackSpeed != value)
            {
                _attackSpeed = value;
                animator.SetFloat(akhAttackSpeed, value);
            }
        }
    }

    bool _attackHeld = false;
    public bool attackHeld
    {
        get { return _attackHeld; }
        set
        {
            _attackHeld = value;
            animator.SetBool(akhAttackHeld, value);
        }
    }

    //Charge-up Attacks
    public bool charging { get; protected set; }

    public AnimationType currentAnimationType { get; protected set; }
    #endregion

    #region Animation Hashes
    //Animator Trigger Hashes
    int akhAbility = Animator.StringToHash("Ability");
    int akhAttack = Animator.StringToHash("Attack");
    int akhThrow = Animator.StringToHash("Throw");
    int akhPain = Animator.StringToHash("Pain");
    int akhStagger = Animator.StringToHash("Stagger");
    int akhReload = Animator.StringToHash("Reload");
    int akhDash = Animator.StringToHash("Dash");

    //Animator Parameter Hashes
    int akhAbilityAnim = Animator.StringToHash("AbilityAnim");
    int akhStaggerAnim = Animator.StringToHash("StaggerAnim");
    int akhDead = Animator.StringToHash("Dead");
    int akhDeadAnim = Animator.StringToHash("DeadAnim");
    int akhSneaking = Animator.StringToHash("Sneaking");
    int akhInteracting = Animator.StringToHash("Interacting");
    int akhFlying = Animator.StringToHash("Flying");
    int akhMotion = Animator.StringToHash("Motion");
    int akhAttackSpeed = Animator.StringToHash("AttackSpeed");
    //int akhAttackAnim = Animator.StringToHash("AttackAnim");
    int akhAttackHeld = Animator.StringToHash("AttackHeld");
    int akhReloadMethod = Animator.StringToHash("ReloadMethod");
    int akhWeaponClass = Animator.StringToHash("WeaponClass");
    #endregion
    
    #region Methods
    //Monobehaviour Methods
    public void Awake()
    {
        //Sometimes awake needs to be called early from Actor class
        if (!awoken)
        {
            myTrans = GetComponent<Transform>();
            animator = GetComponent<Animator>();
            sortingGroup = GetComponent<SortingGroup>();
            spineEquipmentManager = GetComponent<SpineEquipmentManager>();

            awoken = true;
        }
    }
    private void Update()
    {
        if (!dead)
        {
            //Voice timer
            if (voiceTimer > 0) voiceTimer -= Time.unscaledDeltaTime;
        }
    }

    #region Spine Animation Events (ONLY called from Spine)
    /// <param name="parameter">Unused for now</param>
    void attackHitCheck(float parameter)
    {
        if (OnAttackHitCheck != null)
            OnAttackHitCheck(0);
    }
    void cameraShake(float magnitude)
    {
        if (OnCameraShake != null)
            OnCameraShake(magnitude);
    }
    void moveCharacterX(float _direction)
    {
        if (OnMoveCharacterX != null)
            OnMoveCharacterX(_direction);
    }
    void moveCharacterY(float _direction)
    {
        if (OnMoveCharacterY != null)
            OnMoveCharacterY(_direction);
    }
    void playWeaponSound(float soundID)
    {
        if (OnPlayWeaponSound != null)
            OnPlayWeaponSound(Mathf.RoundToInt(soundID));
    }
    void playCharacterSound(float soundID)
    {
        PlayVoiceSound(
            template.GetSound(Mathf.RoundToInt(soundID)),
            true);
    }
    void throwingWeaponRelease(float emitterID)
    {
        if (OnThrowingWeaponRelease != null)
            OnThrowingWeaponRelease(Mathf.RoundToInt(emitterID));
    }
    void executeAbility(float parameter)
    {
        if (OnExecuteAbility != null)
            OnExecuteAbility(specialEffectsEmitter);
    }
    void reloadAmmoAdd(float parameter)
    {
        if (OnReloadAmmoAdd != null)
            OnReloadAmmoAdd();
    }
    void spawnEffect(float effectID)
    {
        Common.SpawnEffect(
            new CommonStructures.EffectSpawnRelative(
                1f,
                GameDatabase.core.effects[Mathf.RoundToInt(effectID)],
                Vector3.zero),
            specialEffectsEmitter,
            direction,
            sortingGroup.sortingOrder + 1);

        //if (OnSpawnEffect != null)
            //OnSpawnEffect(specialEffectsEmitter, Mathf.RoundToInt(effectID));
    }
    #endregion

    //Sound stuff
    protected virtual void PlayVoiceSound(AudioGroupTemplate sound, bool force = true)
    {
        if ((voiceTimer <= 0) || force)
        {
            voiceTimer = voiceDelay;
            AudioManager.instance.Play(sound, myTrans);
        }
    }
    public void PlaySound(CharacterSoundsTemplate.SoundKeys sound, bool force = true)
    {
        PlayVoiceSound(
            template.GetSound((int)sound), 
            force);
    }

    #region State Machine events (ONLY called from StateMachineBehaviour scripts)
    public void StandardAnimatorStateEnter(StateMachineParameters parameters)
    {
        charging = parameters.charging;
        currentAnimationType = parameters.animationType;

        //Fire event
        if (OnStandardAnimatorStateEnter != null)
            OnStandardAnimatorStateEnter(parameters);
    }
    public void StandardAnimatorStateExit(StateMachineParameters parameters)
    {
        //Fire event
        if (OnStandardAnimatorStateExit != null)
            OnStandardAnimatorStateExit(parameters);
    }
    #endregion

    public Transform GetEmitter(float emitterID, AttackType attackType)
    {
        int iEmitterID = Mathf.RoundToInt(emitterID);

        switch (attackType)
        {
            case AttackType.Unarmed:
            case AttackType.MeleeWeapon:
            case AttackType.ThrowingWeapon:
                if (meleeAttackEmitters.Length > 0)
                {
                    if (iEmitterID < meleeAttackEmitters.Length)
                        return meleeAttackEmitters[Mathf.RoundToInt(emitterID)];
                    else
                    {
                        Debug.LogWarning("EmitterID " + iEmitterID + " != meleeAttackEmitters.Length");
                        return meleeAttackEmitters[0];
                    }
                }
                else
                {
                    Debug.LogWarning("No meleeAttackEmitters have been set up!");
                    return null;
                }
            case AttackType.RangedWeapon:
                if (weaponAnchors.Length > 0)
                {
                    if (iEmitterID < weaponAnchors.Length)
                        return weaponAnchors[iEmitterID];
                    else
                    {
                        Debug.LogWarning("Emitter ID " + iEmitterID + " != weaponAnchors.Length");
                        return weaponAnchors[0];
                    }
                }
                else
                {
                    Debug.LogWarning("No weaponAnchors have been set up!");
                    return null;
                }
        }

        //Should never get to this point
        return null;
    }

    #region Control Methods (Called from BaseController)
    public void AttackBegin()
    {
        /*
        attackAnim++;
        if (attackAnim > 1)
            attackAnim = 0;
            */
        animator.SetTrigger(akhAttack);
    }
    public void ThrowingAttackBegin()
    {
        animator.SetTrigger(akhThrow);
    }
    public void Pain()
    {
        //Play sound
        PlaySound(CharacterSoundsTemplate.SoundKeys.hurt, false);

        //Play animation
        animator.SetTrigger(akhPain);
    }
    public void Reload()
    {
        animator.SetTrigger(akhReload);
    }
    public void Stagger()
    {
        //Play sound (TODO: change to stagger sound later)
        PlaySound(CharacterSoundsTemplate.SoundKeys.hurt, false);

        if (staggerAnimations > 1)
        {
            animator.SetInteger(
                akhStaggerAnim,
                UnityEngine.Random.Range(
                    0,
                    staggerAnimations));
        }

        //Play animation
        animator.SetTrigger(akhStagger);
    }
    public void Dash()
    {
        animator.SetTrigger(akhDash);
    }
    public void UseAbility(int abilityAnim)
    {
        animator.SetInteger(akhAbilityAnim, abilityAnim);
        animator.SetTrigger(akhAbility);
    }
    #if UNITY_EDITOR
    public void SetGenericAnimKey(string key)
    {
        animator.SetTrigger(Animator.StringToHash(key));
    }
    public void SetGenericAnimKey(string key, int val)
    {
        animator.SetInteger(Animator.StringToHash(key), val);
    }
    public void SetGenericAnimKey(string key, float val)
    {
        animator.SetFloat(Animator.StringToHash(key), val);
    }
    public void SetGenericAnimKey(string key, bool val)
    {
        animator.SetBool(Animator.StringToHash(key), val);
    }
    #endif
    #endregion
    #endregion
}
