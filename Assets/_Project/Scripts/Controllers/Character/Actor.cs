using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RPGData;

/// <summary>
/// Definition: Objects with which other actors can interact in _some_ way
/// Purpose: To contain/manage all data/methods needed to drive interactions with other actors
/// </summary>
public class Actor : MonoBehaviour
{
    public enum ActionResult { Success = 0, NotReady, NotOwned, NotValid, NotNeeded, NotEnoughHP, NotEnoughMP, NotEnoughAP, }

    #region Constants
    protected const float
        apRegenDelay = 1f,
        hitEffectDelay = 0.1f,
        rubEnemySlowTime = 0.25f,
        statusEffectHandleTime = 1f,
        battleTimeout = 10f,
        regenTimeDelay = 1f,
        throwingTime = 1f,
        bodyFadeTime = 5f,
        dashingSpeedMod = 7.5f;
    protected const int
        //Layers
        layerLoot = 11,
        layerPortal = 14,
        layerNPC = 16,
        layerInteractable = 19,
        //Other
        autoReloadDelay = 1;
    #endregion

    #region Inspector Properties
    [Header("RPG Stats")]
    [Tooltip("Sounds and animations")]
    public CharacterTemplate characterTemplate;
    [Tooltip("Stats, skills, abilities, etc.")]
    public CharacterSheet characterSheet;
    [Tooltip("The assigned CharacterSheet will be cloned at spawn instead of using the currently linked scriptable object\n" +
        "NOTE: Use this if multiple actors use this object as a template, but should have their own (separate) copies during play")]
    public bool cloneCharacterSheet = true;

    [Header("Inventory")]
    [Tooltip("Guaranteed items")]
    public Inventory inventory;
    [Tooltip("Randomized items")]
    public LootTable lootTable;
    [Tooltip("The assigned Inventory will be cloned at spawn instead of using the currently linked scriptable object\n" +
        "NOTE: Use this if multiple actors use this object as a template, but should have their own (separate) copies during play")]
    public bool cloneInventory = true;

    [Header("Behavior")]
    public bool canRun = true;
    [Tooltip("Allowed to make use of 'cover' objects in the environment")]
    public bool canUseCover = false;
    [Tooltip("Can other actors interact [Use] with this actor?")]
    public bool isInteractable = true;
    [Tooltip("Can [Use] other actors/interactable objects")]
    public bool canInteract = true;
    [Tooltip("Actors with the same team cannot damage each other\nAggressive actors will attack any other entity with a different team")]
    public int team;
    [Tooltip("What physics layers should be checked when attacking")]
    public LayerMask attackMask;
    [Tooltip("The distance at which actors of different teams will slow this actor down by proximity")]
    [SerializeField] float rubRadius = 1f;
    [Tooltip("Physics layers to check for proximity slow")]
    [SerializeField] LayerMask slowMask;
    [Tooltip("Used for personal barks and/or conversations")]
    public GameObject conversationPrefab;
    #endregion

    #region Editor-Only Properties
#if UNITY_EDITOR
    [Header("Debugging")]
    public bool killSwitch = false;
    public bool drawHitcheckGizmos = true;
    protected Vector3 lastMeleeHitPosition = Vector3.zero;
    protected Vector3 lastMeleeHitExtents = Vector3.zero;
#endif
    #endregion

    #region Events
    //public System.Action<Actor> AfterInitialized;
    public System.Action<Actor, HitData> OnHit;
    public System.Action<Actor> OnDied;
    public System.Action<Actor, Room> OnChangeRoom;
    public System.Action<Actor> OnTeleported;
    public System.Action<Actor, string> OnGotItems;
    public System.Action<NPCController> OnNearestNPCChanged;
    public System.Action<Actor, InteractableObject, bool> OnInteractableObjectProximityChange;
    public System.Action OnGenerateInventory;
    #endregion

    #region Protected properties
    protected bool
        _inBattle = false;
    protected int
        autoReloadCount = 0;
    protected float
        //Timers
        apUsedTimer = 0f,
        battleTimeoutTimer = 0f,
        chargingTimer = 0f,
        hitEffectTimer = 0f,
        rubbingTimer = 0f,
        sneakCooldownTimer = 0f,
        statusEffectTimer = 0f,
        throwingTimer = 0f,
        weaponTimer = 0f;
    protected Transform myTrans;
    protected SortingGroup sortingGroup;
    protected HitData lastHit;
    protected TDCharacterController2D motionController;
    protected Character character = null;
    protected Collider2D mainCollider = null;

    //Interaction helpers
    protected LootPickup nearestLootPickup = null;
    protected List<LootableObject> lootableObjects = new List<LootableObject>();
    protected List<InteractableObject> interactableObjects = new List<InteractableObject>();
    #endregion

    #region Private Properties
    private bool _sneaking = false;
    private List<Collider> hitBoxes = new List<Collider>();
    private HitBox[] hitBoxComps = null;
    /// <summary>
    /// Used to remember the ability that is being activated while the animation plays
    /// This will be used when the animation calls the executeAbility method to spawn the payload
    /// </summary>
    BaseAbility activatingAbility = null;
    Vector2 constantMotion = Vector2.zero;
    Vector3 meleeHitCheckRange_Defaults = new Vector3(0.5f, 0.25f, 0.01f);
    StateMachineParameters attackStateParameters;
    Fungus.Flowchart _conversationFlowchart = null;
    Character.AttackType currentAttackType
    {
        get
        {
            if (weapon.item == null)
                return Character.AttackType.Unarmed;
            else
            {
                if (weapon.item.template.chargeType > 0)
                {
                    return Character.AttackType.RangedWeapon;
                }
                else
                {
                    return Character.AttackType.MeleeWeapon;
                }
            }
        }
    }
    #endregion

    #region Accessors
    int _dir = 1;
    public virtual int dir
    {
        get { return _dir; }
        set
        {
            if (_dir != value)
            {
                if (motionController)
                    motionController.changeDirection(value);

                if (character)
                    character.direction = value;
            }

            _dir = value;
        }
    }
    bool _dead = false;
    public bool dead
    {
        get { return _dead; }
        private set
        {
            _dead = value;
            ToggleHitBoxes(!value);
        }
    }
    public bool initialized { get; protected set; }
    public bool canAct { get; protected set; }
    public bool allowAutoReload { get; set; }
    bool _invulnerability = false;
    public bool invulnerability
    {
        get { return _invulnerability; }
        set
        {
            _invulnerability = value;
            characterSheet.HP.SetToPercentage(1);
        }
    }
    public bool sneaking
    {
        get { return _sneaking; }
        set
        {
            _sneaking = value;
            character.sneaking = value;
        }
    }
    public virtual bool canSneak
    {
        get
        {
            return sneakCooldownTimer <= 0;
        }
    }
    public Character.Motions motion
    {
        get
        {
            if (character != null)
                return character.motion;

            return 0;
        }
        set
        {
            if (character != null)
                character.motion = value;
        }
    }
    public float speed
    {
        get
        {
            if (sneaking)
                return characterSheet.speedSneak;
            else
                if (motion == Character.Motions.Run)
                return characterSheet.speedRun;
            else
                return characterSheet.speedBase;
        }
    }
    public float animationMotionMultiplier { get; protected set; }
    public virtual bool inBattle
    {
        get { return _inBattle; }
        set
        {
            if ((value) && (!dead))
                battleTimeoutTimer = battleTimeout;

            if (_inBattle != value)
            {
                _inBattle = value;

                if (!dead && !value)
                    battleTimeoutTimer = 0;
            }
        }
    }
    public bool busy { get { return character.currentAnimationType != AnimationType.Motion; } }
    public bool controlledByPlayer { get; set; }
    public InteractableObject interacableObjectInUse { get; protected set; }
    public Vector2 controllerOffset { get { return character.controllerOffset; } }
    public Vector2 characterWidthBySide { get { return character.characterWidthBySide; } }
    public Vector2 floatingObjectOffset { get { return character.floatingObjectOffset; } }
    public BaseItem throwingWeapon { get; private set; }
    public BaseWeapon weapon { get; protected set; }
    public BaseItem ammoCache { get; protected set; }
    public DepthHost depthHost { get; protected set; }
    Room _currentRoom = null;
    public virtual Room currentRoom
    {
        get
        {
            //If for any reason the room is not set, try to detect one
            //(Usually needed for level testing)
            if (!_currentRoom)
                currentRoom = Room.Detect(myTrans);

            return _currentRoom;
        }
        set
        {
            if (_currentRoom != value)
            {
                if (_currentRoom != null)
                {
                    //Notify room that entity left
                    _currentRoom.EntityLeftRoom(this);
                }

                _currentRoom = value;

                if (value != null)
                {
                    //Notify room that entity entered
                    value.EntityEnteredRoom(this);
                }

                //Depth Controller needs to update Depth Host from new room
                //if (depthController) depthController.HostChanged();
                depthHost = DepthHost.Find(myTrans);

                //Fire Event
                if (OnChangeRoom != null)
                    OnChangeRoom(this, value);
            }
        }
    }
    public CharacterSoundsTemplate characterSounds { get { return character.template.soundsTemplate; } }
    public Fungus.Flowchart conversationFlowchart 
    { 
        get 
        { 
            if (_conversationFlowchart == null) 
                _conversationFlowchart = Instantiate(conversationPrefab, myTrans).GetComponent<Fungus.Flowchart>();

            return _conversationFlowchart;
        }
    }
    #endregion

    #region Methods
    protected virtual void Awake()
    {
        PreInit();
    }
    protected virtual void Update()
    {
        UpdateTimers();
        if (initialized && !dead)
        {
            UpdateActiveTimers();

            //Debug kill switch
            if (killSwitch)
            {
                //Don't award player exp for these kills
                lastHit.fromPlayer = false;

                //Zero out the HP
                characterSheet.HP.SetToFixedAmount(0);

                //Unset the kill switch so this code doesn't keep running every frame
                killSwitch = false;
            }
        }
    }
    protected virtual void OnDestroy()
    {
        CharacterSheetEvents_Unsubscribe();

        //Attempt at helping the Garbage Collector do a better job
        myTrans = null;
        motionController = null;
        ammoCache = null;
        weapon.Equip(null, null);
        character = null;
        lootableObjects.Clear();
        lootableObjects = null;
        interactableObjects.Clear();
        interactableObjects = null;
    }

    //Startup
    void PreInit()
    {
        canAct = false;
        dead = false;
        initialized = false;

        //Cache components
        myTrans = transform;
        motionController = GetComponent<TDCharacterController2D>();
        mainCollider = GetComponent<Collider2D>();
        sortingGroup = GetComponentInChildren<SortingGroup>();

        if (inventory == null)
            //Create new if none assigned
            inventory = ScriptableObject.CreateInstance<Inventory>();
        else if (cloneInventory)
            //Make a virtual copy from the physical asset
            inventory = ScriptableObject.Instantiate<Inventory>(inventory);

        if (!characterSheet)
            //Create new if none assigned
            characterSheet = ScriptableObject.CreateInstance<CharacterSheet>();
        else if (cloneCharacterSheet)
            //Make a virtual copy from the physical asset
            ScriptableObject.Instantiate<CharacterSheet>(characterSheet);

        //Did we spawn backwards?
        if (myTrans.localScale.x < 0) _dir = -1;

        //Cache components
        character = GetComponentInChildren<Character>();
        character.Awake();
        character.direction = dir;
        character.template = characterTemplate;

        //Subscribe to events
        CharacterEvents_Subscribe();

        //Cache hitboxes
        CacheHitBoxes();

        weapon = new BaseWeapon(sortingGroup);

        //Cache the weapon slot position in the equipslot array
        weapon.ChangeSlot(character.GetComponent<SpineEquipmentManager>()
            .getSlot(SpineEquipmentManager.EquipSlotNames.Weapon));

        //Stay still
        constantMotion = Vector2.zero;

        //Reset stuff that may be set from earlier spawn
        animationMotionMultiplier = 1f;

        //Subscribe to character sheet events
        CharacterSheetEvents_Subscribe();

        //Do very first stat calculation
        characterSheet.CalcStats();

        //Turn on main collider
        if (mainCollider) mainCollider.enabled = true;

        //Mark actor as ready to play
        initialized = true;
        canAct = true;

        //Create starting items, equip them, etc
        GenerateInventory();

        //Put back on equipment (if applicable)
        ReEquip();

        //Fire Event
        //if (AfterInitialized != null)
            //AfterInitialized(this);
    }
    void GenerateInventory()
    {
        //Generate loot from table
        if (lootTable != null)
            inventory.TransferFrom(lootTable.makeLoot(0));

        //Equip items
        if (characterTemplate.equipArmor)
            ChangeAllEquipment(true, true, new ItemTypes[] { ItemTypes.Apparel });
        if (characterTemplate.equipWeapons)
            ChangeAllEquipment(true, true, new ItemTypes[] { ItemTypes.Weapon });

        if (OnGenerateInventory != null) OnGenerateInventory();
    }
    void Died()
    {
        //Toggle Flag
        dead = true;

        inBattle = false;
        character.dead = true;
        motion = Character.Motions.Idle;
        constantMotion = Vector2.zero;

        characterSheet.RemoveAllStatusEffects(false);

        //Turn off main collider
        if (mainCollider) mainCollider.enabled = false;

        //Remove entity from room
        currentRoom = null;

        //Fire Event
        if (OnDied != null)
            OnDied(this);
    }
    void CacheHitBoxes()
    {
        //Clear any previous cache data
        hitBoxes.Clear();

        //Find hitboxes on current character model
        hitBoxComps = GetComponentsInChildren<HitBox>();

        //Cache colliders for quick-search
        foreach (HitBox hitbox in hitBoxComps)
            hitBoxes.Add(hitbox.GetComponent<Collider>());

        //Wire up hit events and team cache
        foreach (HitBox HB in hitBoxComps)
        {
            HB.team = team;
            HB.OnHit += Hit;
        }
    }
    void SlowCheck()
    {
        if (rubbingTimer <= 0)
        {
            if (Physics.CheckSphere(
                myTrans.position,
                rubRadius,
                slowMask.value))
            {
                rubbingTimer = rubEnemySlowTime;
            }
        }
    }
    void HPChanged(float newValue, float delta, float overflow)
    {
        if (!dead) if (newValue <= 0) Died();
    }
    void APChanged(float newValue, float delta, float overflow)
    {
        //Reset delay timer when AP is used
        if (delta < -0.01f)
            apUsedTimer = apRegenDelay;
    }
    void SPChanged(float newValue, float delta, float overflow)
    {
        //Doing any of this while dead could cause bugs
        if (!dead && (overflow > 0.01f))
        {
            //Extra SP goes into XP
            GiveXP(overflow);
        }
    }
    public void GiveXP(float amount)
    {
        characterSheet.XP += amount;
    }
    public void Teleported()
    {
        //Re-dected the room in case of change
        currentRoom = Room.Detect(myTrans);

        //Fire event
        if (OnTeleported != null)
            OnTeleported(this);
    }
    public void Hit(HitData hitData)
    {
        //Were we dead before the hit?
        bool deadBefore = dead;

        if (!dead)
        {
            //Cache hitData
            lastHit = hitData;

            //Take invulnerability into consideration
            if (invulnerability)
                hitData.damageModifier = 0f;

            //Calculate damage done against protection
            lastHit.CalculateDamage(characterSheet.protection);

            //Adjust HP
            if (lastHit.damageResult.value > 0f)
                characterSheet.HP.Consume(lastHit.damageResult.value);

            //Was the rounded damage enough?
            if (lastHit.damageResult.value > 0f)
            {
                //These things should only happen if the player is free to do things because they could potentially interrupt other more critical stuff
                if ((!dead) && (canAct))
                {
                    //Check for stagger shot
                    if (characterSheet.StaggerCheck(lastHit.damageResult.value))
                        character.Stagger();
                    else
                        character.Pain();
                }

                //Shake the camera when crits happen
                if (lastHit.damageResult.crit)
                    AddCameraShake(10f);

                //Show damage numbers if that game option is on
                if (GameDatabase.sGameSettings.showDamageNumbers)
                {
                    //Construct the name of the bodypart/hitbox that was struck
                    string hitPart = hitData.hitBox == null ?
                        "Somewhere" :
                        hitData.hitBox.bodyPartName;

                    //Construct damage numbers
                    string floatText = string.Concat(
                        lastHit.damageResult.value.ToString(Common.defaultIntFormat),
                        " (", hitPart, ")",
                        lastHit.damageResult.crit ? " *Crit*" : string.Empty);

                    //Send request to make floating text appear
                    GameDatabase.sGameSettings.FloatSomeText(
                        floatText,
                        Color.red,
                        hitData.hitPosition);
                }
            }

            //Generate hit effects
            DoHitEffects(
                hitData.hitPosition,
                lastHit.damageResult.crit);

            //if (characterSheet.HP.valueCurrent > 0)
            //if(!dead)
            inBattle = true;

            //Fire event
            if (OnHit != null) OnHit(this, hitData);
        }
    }
    public void Resurrect()
    {
        characterSheet.HP.SetToPercentage(0.1f);
        characterSheet.MP.SetToPercentage(0.1f);
        characterSheet.SP.SetToPercentage(0.1f);
    }

    //Reaction handling
    protected void DoHitEffects(Vector3 position, bool crit)
    {
        GameObject prefab = crit ?
            characterTemplate.criticalHitEffect :
            characterTemplate.hitEffect;

        //Call base function (which doesn't care about crits)
        DoHitEffects(position, prefab);
    }

    //Event subscription management
    void CharacterEvents_Subscribe()
    {
        character.OnAttackHitCheck += AttackHitCheckEventHandler;
        character.OnStandardAnimatorStateEnter += StandardAnimationStateEnter;
        character.OnStandardAnimatorStateExit += StandardAnimationStateExit;
        character.OnReloadAmmoAdd += ReloadMid;
        character.OnThrowingWeaponRelease += ThrowingWeaponRelease;
        character.OnExecuteAbility += ExecuteAbility;
        //character.OnPlayCharacterSound += PlayCharacterSound;
        character.OnPlayWeaponSound += PlayWeaponSound;
        character.OnMoveCharacterX += SetConstantMotionX;
        character.OnMoveCharacterY += SetConstantMotionY;
        character.OnCameraShake += AddCameraShake;
    }
    void CharacterEvents_Unsubscribe()
    {
        character.OnAttackHitCheck -= AttackHitCheckEventHandler;
        character.OnStandardAnimatorStateEnter -= StandardAnimationStateEnter;
        character.OnStandardAnimatorStateExit -= StandardAnimationStateExit;
        character.OnReloadAmmoAdd -= ReloadMid;
        character.OnThrowingWeaponRelease -= ThrowingWeaponRelease;
        character.OnExecuteAbility -= ExecuteAbility;
        //character.OnPlayCharacterSound -= PlayCharacterSound;
        character.OnPlayWeaponSound -= PlayWeaponSound;
        character.OnMoveCharacterX -= SetConstantMotionX;
        character.OnMoveCharacterY -= SetConstantMotionY;
        character.OnCameraShake -= AddCameraShake;
    }
    void CharacterSheetEvents_Subscribe()
    {
        if (!characterSheet) return;
        characterSheet.HP.OnCurrentValueChanged += HPChanged;
        characterSheet.AP.OnCurrentValueChanged += APChanged;
        characterSheet.SP.OnCurrentValueChanged += SPChanged;
    }
    void CharacterSheetEvents_Unsubscribe()
    {
        if (!characterSheet) return;
        characterSheet.HP.OnCurrentValueChanged -= HPChanged;
        characterSheet.AP.OnCurrentValueChanged -= APChanged;
        characterSheet.SP.OnCurrentValueChanged -= SPChanged;
    }

    //Inventory
    public bool ChangeEquipment(BaseItem item, bool putItOn, bool overideBusy = false)
    {
        if (item == null)
            return false;

        //Prevent switch if busy
        if ((!overideBusy) && ((item.template.isWeapon()) && (busy)))
            return false;

        //Equip item
        inventory.EquipItem(item, character, putItOn);

        //Weapon-specific tasks
        if (item.template.isWeapon())
            ChangeWeapon(item, putItOn);

        //ThrowingWeapon-specific tasks
        if (item.template.isThrowingWeapon())
            ChangeThrowingWeapon(item, putItOn);

        //Most equipment will alter stats on the character sheet.
        characterSheet.CalcStats();

        //Set character animation weapon class & reload method
        if (weapon.item == null)
        {
            character.weaponAnimationKey = 0;
            //TODO: Set this based on charactersheet stats
            character.attackSpeed = 1;
        }
        else
        {
            character.reloadMethod = weapon.template.reloadMethod;
            character.attackSpeed = 1 / weapon.item.template.useDelay;
            character.weaponAnimationKey =
                characterTemplate.weaponAnimationProfile.GetAnimationKey(
                    weapon.item.template);
        }

        return true;
    }
    /// <summary>
    /// Searches for specified item types from the inventory, then either equips OR removes them depending on the putItOn property
    /// Usually called on NPCs and Enemies that should auto-equip items from a pre-designed inventory or generated from a loot table
    /// </summary>
    /// <param name="putItOn">true = equip, false = unequip</param>
    /// <param name="overideBusy">carry out operation even if actor is too bust to normally equip/unequip anything</param>
    /// <param name="inclusionList">Item types that should be equipped/unequipped</param>
    /// <param name="equipEmpty">false = ignore weapons that need ammo AND do not have any</param>
    public void ChangeAllEquipment(bool putItOn = true, bool overideBusy = false, ItemTypes[] inclusionList = null, bool equipEmpty = false)
    {
        List<BaseItem> equipList = new List<BaseItem>();
        List<BaseItem> weaponList = new List<BaseItem>();

        bool equipWeapons =
            (inclusionList != null) &&
            (System.Array.IndexOf(inclusionList, ItemTypes.Weapon) >= 0);

        //Build lists of potentially equippable items
        foreach (BaseItem item in inventory.items)
        {
            //Build non-weapon list
            if (item.template.itemType != ItemTypes.Weapon)
            {
                if (item.equipped != putItOn)
                    if ((inclusionList == null) || (System.Array.IndexOf(inclusionList, item.template.itemType) >= 0))
                        equipList.Add(item);
            }

            //Build weapon list
            if (equipWeapons && (item.template.itemType == ItemTypes.Weapon))
            {
                if (item.equipped != putItOn)
                    weaponList.Add(item);
            }
        }

        //Equip non-weapons
        foreach (BaseItem item in equipList)
            ChangeEquipment(item, putItOn, overideBusy);

        //If putting on a weapon, do extended checking
        if (putItOn)
        {
            //Start off with worst weapon (unarmed)
            BaseItem bestWeapon = null;

            //Select best weapon from inventory
            foreach (BaseItem item in weaponList)
            {
                //Cache
                bool rangedWeapon =
                    item.template.isRangedWeapon();
                bool hasAmmo =
                    (!rangedWeapon) ||
                    (item.chargesLeft > 0) ||
                    (inventory.FindAmmo(item.template.chargeType) != null);

                //Skip empty weapons
                if (equipEmpty || hasAmmo)
                {
                    //Any weapon is better than nothing
                    if (bestWeapon == null)
                    {
                        bestWeapon = item;
                        continue;
                    }

                    //Any Ranged Weapon is better than a melee weapon
                    //TODO: Use ranged check for AIs to select melee when close to player
                    if (bestWeapon.template.isMeleeWeapon() && rangedWeapon)
                    {
                        bestWeapon = item;
                        continue;
                    }

                    //Use most powerful weapon
                    if (bestWeapon.template.power < item.template.power)
                    {
                        bestWeapon = item;
                        continue;
                    }
                }
            }

            ChangeEquipment(bestWeapon, true, overideBusy);
        }
        //Taking off is very simple
        else
        {
            ChangeEquipment(weapon.item, false, overideBusy);
        }
    }
    /// <summary>
    /// Puts back on any equipment in the inventory marked as "equipped"
    /// Usually called after loading invetory data from a saved state or otherwise changing inventory in a non-standard way
    /// </summary>
    private void ReEquip()
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i].equipped)
                ChangeEquipment(inventory.items[i], true, true);
        }
    }
    protected virtual void ChangeWeapon(BaseItem item, bool putItOn)
    {
        //Clean up potential garbage collector block of weapon.item
        weapon.Equip(null, null);

        if (putItOn)
        {
            //Setup weapon from item and template
            weapon.Equip(item, item.template.weaponTemplate);

            //Notify ammo-type changed
            SetUsedAmmo(inventory.FindAmmo(item.template.chargeType));
        }
    }
    protected virtual void ChangeThrowingWeapon(BaseItem item, bool putItOn)
    {
        //Cache throwing weapon for efficient use during combat
        if (putItOn)
            throwingWeapon = item;
        else
            throwingWeapon = null;
    }
    public LootPickup FindNearbyLootBag()
    {
        //Is one already cached?
        if (nearestLootPickup != null)
            return nearestLootPickup;

        //Search the list of touching interactables for loot bags
        foreach (InteractableObject io in interactableObjects)
        {
            LootPickup ret = io.GetComponent<LootPickup>();
            if (ret != null) return ret;
        }

        //None found
        return null;
    }
    protected void DropItem(BaseItem item, int quantity)
    {
        Inventory droppedItems = new Inventory();
        droppedItems.AddItem(item, 1);
        inventory.RemoveItem(item, 1);
        DropInventory(droppedItems);
    }
    protected void DropInventory(Inventory inventoryToDrop)
    {
        //Drop contents of inventory on the ground as a pickup
        if ((inventoryToDrop != null) && (!inventoryToDrop.IsEmpty()))
        {
            //Find an existing bag
            LootPickup loot = FindNearbyLootBag();

            //Make a new bag if none found
            if (loot == null)
            {
                //Spawn new lootbag prefab
                loot = Common.poolSpawn(GameDatabase.core.lootBagPrefab).GetComponent<LootPickup>();

                //Place the new lootbag at our feet
                loot.transform.position = myTrans.position;

                //Cache the lootbag in case we want to use it again (dropping multiple items)
                nearestLootPickup = loot;
            }

            //Put items into loot bag
            inventoryToDrop.TransferTo(loot.inventory);
        }
    }
    protected virtual bool UseItem(BaseItem item)
    {
        if (item != null)
        {
            switch (item.template.itemType)
            {
                case ItemTypes.Consumable:
                    characterSheet.AddStatusEffect(item.template);
                    inventory.RemoveItem(item, 1);
                    break;
                case ItemTypes.Weapon:
                case ItemTypes.Apparel:
                case ItemTypes.Throwable:
                    ChangeEquipment(item, !item.equipped);
                    break;
                case ItemTypes.Book:
                    LearnAbility(item);
                    break;
                default:
                    //Play error sound
                    break;
            }

            return true;
        }
        return false;
    }
    protected virtual void DiscardItem(BaseItem item)
    {
        if ((item.equipped) && (item.quantity == 1))
            ChangeEquipment(item, false);

        DropItem(item, 1);
    }

    //Actions that can be invoked from controllers
    public ActionResult Attack()
    {
        //What type of attack?
        if ((weapon.item == null) || (weapon.template.weaponType == WeaponTemplate.WeaponTypes.Melee))
        {
            //Melee attacks cancel sneaking
            if (sneaking)
            {
                sneaking = false;
                sneakCooldownTimer = 0.25f;
            }

            //Melee attacks
            if (characterSheet.AP.valueCurrent > 0)
            {
                character.AttackBegin();
                return ActionResult.Success;
            }
            else
            {
                return ActionResult.NotEnoughAP;
            }
        }
        else
        {
            //Ranged weapon attacks
            AttackRangedBegin();
            return ActionResult.Success;
        }
    }
    public ActionResult Move(float hor, float ver, bool run)
    {
        if (!initialized) return ActionResult.NotReady;

        SlowCheck();

        //Is there any input?
        if ((((hor != 0) || (ver != 0)) && (animationMotionMultiplier != 0)))
        {
            //Run or walk?
            motion = (run && canRun) ? Character.Motions.Run : Character.Motions.Walk;

            //Slowed by proximity?
            if (rubbingTimer > 0)
            {
                motion = Character.Motions.Limp;
                hor *= 0.5f;
                ver *= 0.5f;
            }

            //Apply animation modifiers
            hor *= animationMotionMultiplier;
            ver *= animationMotionMultiplier;

            //Flip object when moving in the opposite direction.
            if (hor != 0) dir = (hor > 0) ? 1 : -1;

            //Send calculated parameters to movetion controller
            //float depthOffset = 1f + (depthController == null ? 0f : depthController.hostScaleOffset);
            float depthOffset = 1f + (depthHost == null ? 0f : depthHost.scaleOffset);
            MotionControllerMove(
                new Vector2(hor, ver),
                speed * depthOffset);
        }
        else
        {
            //Stop character if moving.
            motion = Character.Motions.Idle;
        }

        //Motions from non-input sources
        if (constantMotion != Vector2.zero)
        {
            //Apply constant motion
            MotionControllerMove(constantMotion, characterSheet.speedBase);
        }

        return ActionResult.Success;
    }
    public ActionResult Dash()
    {
        if (!busy)
        {
            if (characterSheet.AP.valueCurrent > 0)
            {
                character.Dash();
                characterSheet.AP.Consume(20f);
                return ActionResult.Success;
            }
            else
            {
                return ActionResult.NotEnoughAP;
            }
        }
        else
        {
            return ActionResult.NotReady;
        }
    }
    public ActionResult UseItem(BaseItem item, SubmitState state = SubmitState.Normal)
    {
        if (inventory.items.Contains(item))
        {
            if (state == SubmitState.Negative)
                DiscardItem(item);
            else
                UseItem(item);

            return ActionResult.Success;
        }
        else
        {
            Debug.LogError("Item is not present in my inventory!");
            return ActionResult.NotOwned;
        }
    }
    public ActionResult DropInventory()
    {
        if (inventory.items.Count > 0)
        {
            //Remove any equipped items
            ChangeAllEquipment(false, true, 
                new ItemTypes[] { ItemTypes.Apparel, ItemTypes.Weapon });

            //Drop all items on the ground
            DropInventory(inventory);

            //Report success
            return ActionResult.Success;
        }
        else
            return ActionResult.NotOwned;
    }
    public ActionResult Reload()
    {
        //Validation checklist
        if ((weapon.item == null) || (weapon.item.template.chargeType < 1))
            return ActionResult.NotValid;
        if (weapon.item.chargesLeft == weapon.item.template.chargeCount)
            return ActionResult.NotNeeded;
        if (!weapon.canReload)
            return ActionResult.NotReady;
        BaseItem ammo = inventory.FindAmmo(weapon.item.template.chargeType);
        if (ammo == null) return ActionResult.NotOwned;

        //Finally we can actually reload!
        int ammoNeeded = (weapon.item.template.chargeCount - weapon.item.chargesLeft);
        int ammoToPutIn = 0;

        if (ammo.quantity > ammoNeeded)
            ammoToPutIn = ammoNeeded;
        else
            ammoToPutIn = ammo.quantity;

        weapon.ammoCountToReloadWith = ammoToPutIn;

        //Start animation
        character.Reload();

        return ActionResult.Success;
    }
    public ActionResult ActivateAbility(BaseAbility ability, bool useMP, bool useAP, bool showAlerts = false)
    {
        //TOFIX: If an ability is used just as sneak is dropped it will cause activatingAbility to get stuck non-null and prevent further ability use

        //Validation checks
        //Can't use ability while sneaking
        if (sneaking) return ActionResult.NotReady;
        //Is the ability on cooldown?
        if (!ability.isReady()) return ActionResult.NotReady;
        //Do we have enough MP?
        if (useMP && !characterSheet.MP.HasEnough(ability.template.mpcost))
            return ActionResult.NotEnoughMP;
        if (useAP && !characterSheet.AP.HasEnough(ability.template.apcost))
            return ActionResult.NotEnoughAP;

        //Cache ability for animation sequencer
        activatingAbility = ability;
        //Set ability to cooldown
        activatingAbility.used(characterSheet.learningModifier);
        //Consume MP
        characterSheet.MP.Consume(ability.template.mpcost);
        //Consume AP
        characterSheet.AP.Consume(ability.template.apcost);
        //Play animation
        character.UseAbility(
            System.Array.Find(
                characterTemplate.animationProfile.abilityAnimations,
                x => x.ability == ability.template)
            .animationKey);

        return ActionResult.Success;
    }
    public ActionResult UseThrowingWeapon()
    {
        ActionResult result = CanUseThrowingWeapon();

        if (result == ActionResult.Success)
        {
            //Set timer
            throwingTimer = throwingTime;

            //Initiate throwing animation (the rest is handled when the animation triggers the throw frame)
            character.ThrowingAttackBegin();
        }

        return result;
    }

    //Attack Methods
    /// <summary>
    /// Checks for hitboxes close enough to the origin
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="colliders"></param>
    /// <returns></returns>
    Collider GetNearestCollider(Vector3 origin, Collider[] colliders)
    {
        Collider ret = colliders[0];

        float lastDistance =
            colliders[0] == null ? 999f :
            Vector3.Distance(origin, colliders[0].transform.position);

        for (int i = 1; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                float distance = Vector3.Distance(origin, colliders[i].transform.position);
                if ((ret == null) || (lastDistance > distance))
                {
                    ret = colliders[i];
                    lastDistance = distance;
                }
            }
        }

        return ret;
    }
    /// <summary>
    /// Attached to Character.OnAttackHitCheck
    /// This is the point during the attack animation where the hit is calculated
    /// </summary>
    /// <param name="parameter">Parameter from Spine animation event call</param>
    protected void AttackHitCheckEventHandler(int emitterID)
    {
        //Fetch emitter
        Transform emitter = character.GetEmitter(emitterID, currentAttackType);

        //Determine which type of hitcheck to run
        switch (currentAttackType)
        {
            case Character.AttackType.Unarmed:
                AttackMeleeHitCheck(emitter, false);
                break;
            case Character.AttackType.MeleeWeapon:
                AttackMeleeHitCheck(emitter, true);
                break;
            case Character.AttackType.RangedWeapon:
                AttackRangedHitCheck(emitter);
                break;
        }
    }
    protected void AttackMeleeHitCheck(Transform emitter, bool usingWeapon)
    {
        float hitRangeModifier = 3f;

        //Calculate damage modification
        float damageMultiplier =
            attackStateParameters.damageMultiplier +
            Mathf.Clamp(
                attackStateParameters.chargeMultiplier * chargingTimer,
                0,
                attackStateParameters.maxChargeMultiplier);

        Vector3 hitBoxExtents = new Vector3(
            meleeHitCheckRange_Defaults.x * hitRangeModifier,
            meleeHitCheckRange_Defaults.y,
            meleeHitCheckRange_Defaults.z)
            +
            attackStateParameters.hitboxSizeOffset;

        Vector3 emissionPos = new Vector3(
            emitter.position.x - (hitRangeModifier / 4f) * dir,
            emitter.position.y,
            emitter.position.z);

#if UNITY_EDITOR
        lastMeleeHitPosition = emissionPos;
        lastMeleeHitExtents = hitBoxExtents;
#endif

        //Check to see what was hit
        Collider[] hits = Physics.OverlapBox(
            emissionPos,
            hitBoxExtents,
            emitter.rotation,
            attackMask.value);

        if (hits.Length > 0)
        {
            if (usingWeapon)
            //Weapon
            {
                RegisterWeaponHits(
                    null,
                    hits,
                    emitter.position,
                    damageMultiplier);
            }
            //Unarmed
            else
            {
                //Send the hit info to the object that was hit
                HitData hitinfo = new HitData(
                    controlledByPlayer,
                    characterSheet.damage,
                    damageMultiplier,
                    characterSheet.critRate,
                    2f,
                    emitter.position,
                    null,
                    new DamageResult());

                //Don't hit myself (remove my own colliders from the list)
                for (int i = 0; i < hits.Length; i++)
                    if (IsHitBoxMineOrTeams(hits[i]))
                        hits[i] = null;

                //Fetch the collider nearest to the origin
                Collider colliderHit = GetNearestCollider(emitter.position, hits);
                if (colliderHit != null)
                    //Tell the collider it has been hit
                    colliderHit.SendMessage(
                        HitBox.msgOnAttacked,
                        hitinfo,
                        SendMessageOptions.DontRequireReceiver);
            }

            inBattle = true;
        }
    }
    protected void AttackRangedHitCheck(Transform emitter)
    {
        //RaycastHit singleHit;
        RaycastHit[] rangedHits = null;
        Collider[] pointBlankHits = null;
        Vector3 shotDirection, emitterPosition;

        float
            //Distance to hit / max weapon range / point blank distance (default)
            hitDist,
            //Angle of shot
            angle = 0,
            //Angle dampening (based on character stats)
            angleModifier = characterSheet.rangedWeaponShotAngleModifier;

        //Play firing sound (temporary code placement - use animation event!)
        AudioManager.instance.Play(weapon.template.sounds.fire, myTrans);

        //Decrement the amount of ammo remaining
        weapon.item.chargesLeft -= weapon.item.template.chargeUse;

        //Do hit checks
        foreach (Vector3 emitterOffset in weapon.template.attackEmitters)
        {
            emitterPosition = Common.calcDirectionalOffset(emitter.position, emitterOffset, dir);
            weapon.DoFireEffects(emitter, dir);

            //Cast a ray for each pellet
            for (int i = 0; i < weapon.item.template.shotCount; i++)
            {
                //Setup shot properties
                if (weapon.item.template.shotSpread > 0)
                    angle = angleModifier * UnityEngine.Random.Range(-weapon.item.template.shotSpread / 2, weapon.item.template.shotSpread / 2);

                shotDirection = Quaternion.AngleAxis(angle, Vector3.forward) * (Vector3.right * dir);

                //Set point-blank hit distance for tracers
                hitDist = 0.1f;

                //Get point-blank hits (rays that start inside a collider do not detect that collider)
                pointBlankHits = Physics.OverlapBox(
                    emitterPosition,
                    Vector3.one * 0.01f,
                    Quaternion.identity,
                    attackMask.value);

                //Get ranged hits
                rangedHits = Physics.RaycastAll(emitterPosition, shotDirection, weapon.item.template.range, attackMask.value);
                /*
                if (weapon.item.template.piercingFactor > 0)
                {
                    //Get all targets along the line
                    rangedHits = Physics.RaycastAll(emitterPosition, shotDirection, weapon.item.template.range, attackMask.value);
                }
                else
                {
                    //Optimization: Only get closest target along the line
                    if (Physics.Raycast(emitterPosition, shotDirection, out singleHit, weapon.item.template.range, attackMask.value))
                        if (singleHit.collider)
                            rangedHits = new RaycastHit[1] { singleHit };
                }
                */

                //Send hit notifications to the target(s)
                RegisterWeaponHits(
                    rangedHits,
                    pointBlankHits,
                    emitterPosition,
                    1f);

                //Set distance for tracer
                hitDist = CalcWeaponHitDistance(
                    weapon.item.template.piercingFactor,
                    weapon.item.template.range,
                    emitterPosition,
                    pointBlankHits,
                    rangedHits);

                //Draw tracer effect(s)
                weapon.DoTracerEffects(
                    emitter,
                    emitterPosition,
                    emitterPosition + (shotDirection * hitDist),
                    dir);

                //Spawn ejection effects
                if (weapon.template.ejectionMethod == WeaponTemplate.EjectionMethods.OnFireAll)
                    weapon.DoEjectionEffects(emitter, dir, myTrans.position.y);

                //Notify the inventory manager delegates that ammo may have been used
                inventory.NotifySingleItemChanged(weapon.item, false);
            }
        }

        if (weapon.template.attackEmitters.Length > 0)
            if (weapon.template.ejectionMethod == WeaponTemplate.EjectionMethods.OnFireOnce)
                weapon.DoEjectionEffects(emitter, dir, myTrans.position.y);
    }
    protected bool AttackRangedBegin()
    {
        bool attacked = false;

        if ((weaponTimer <= 0) && (weapon.canFire) &&
            (weapon.currentState != BaseWeapon.WeaponState.Reloading) &&
            (weapon.currentState != BaseWeapon.WeaponState.ReloadEnd))
        {
            weaponTimer = weapon.item.template.useDelay;

            if (weapon.item.chargesLeft >= weapon.item.template.chargeUse)
            {
                character.AttackBegin();
                attacked = true;
                autoReloadCount = 0;
            }
            else
            {
                if (weapon.currentState != BaseWeapon.WeaponState.Reloading)
                {
                    if (!GameDatabase.sGameSettings.autoReload || (autoReloadCount < autoReloadDelay))
                    {
                        //weapon.currentState = BaseWeapon.WeaponState.DryFiring;

                        //playWeaponSound(sounds.empty);
                        AudioManager.instance.Play(weapon.template.sounds.empty, myTrans);
                        autoReloadCount++;
                    }
                    else AutoReload();
                }
                else autoReloadCount = 0;
            }
        }

        return attacked;
    }
    protected float CalcWeaponHitDistance(float piercingFactor, float maxRange, Vector3 emitterPosition, Collider[] pointBlankHits, RaycastHit[] rangedHits)
    {
        //Shot travels full weapon range distance by piercing all objects it hits
        //nb: does not neccessarily damage all of them
        if (piercingFactor > 0)
            return maxRange;

        //Shot stopped by the object it hit at the muzzle
        if ((pointBlankHits != null) && (pointBlankHits.Length > 0))
            return 0;

        //Shot stopped by the object it hit at a distance
        if ((rangedHits != null) && (rangedHits.Length > 0))
            return Vector3.Distance(emitterPosition, rangedHits[0].point);

        return maxRange;
    }
    protected void RegisterWeaponHits(RaycastHit[] rangedHits, Collider[] pointBlankHits, Vector3 emitterPosition, float damageMultiplier)
    {
        //How many objects the piercing shot has already gone through
        int hitCount = 0;

        //Process pointBlankHits first in case piercing factor is not enough to get through all the enemies
        if ((pointBlankHits != null) && (pointBlankHits.Length > 0))
        {
            //Order pointBlankHits by distance
            pointBlankHits = pointBlankHits.OrderBy(h => h.transform.position.x - emitterPosition.x).ToArray();
            foreach (Collider hit in pointBlankHits)
            {
                //Don't hit myself
                if (!IsHitBoxMineOrTeams(hit))
                {
                    SendWeaponHitMessage(
                        hit,
                        emitterPosition,
                        hitCount,
                        characterSheet.damage * damageMultiplier);

                    hitCount++;
                }
            }
        }

        //Process rangedHits next
        if ((rangedHits != null) && (rangedHits.Length > 0))
        {
            //Order rangedHits by distance
            rangedHits = rangedHits.OrderBy(h => h.distance).ToArray();
            foreach (RaycastHit hit in rangedHits)
            {
                //Don't hit myself
                if (!IsHitBoxMineOrTeams(hit.collider))
                {
                    SendWeaponHitMessage(
                        hit.collider,
                        hit.point,
                        hitCount,
                        characterSheet.damage * damageMultiplier);

                    hitCount++;
                }
            }
        }

    }
    protected void SendWeaponHitMessage(Collider receiver, Vector3 hitPoint, int hitCount, ForceMatrix damage)
    {
        //Calculate piercing factor (if applicable)
        float piercingFactor = Mathf.Pow(weapon.item.template.piercingFactor, hitCount);

        //Don't bother if the damage is practically 0
        if ((weapon.item.template.piercingFactor == 0f) || (piercingFactor > 0.01f))
        {
            //Tell hitbox it was hit
            receiver.SendMessage(
                HitBox.msgOnAttacked,
                new HitData(
                    controlledByPlayer,
                    damage,
                    piercingFactor,
                    characterSheet.critRate,
                    2f,
                    hitPoint,
                    null,
                    new DamageResult()),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    //Movement Methods
    /// <summary>
    /// Modifies speed based on inventory load
    /// </summary>
    /// <returns></returns>
    protected float GetWeightSpeedMod()
    {
        float weightRatio = inventory.weight / characterSheet.carryWeight;
        float speedMod = 1;

        if (weightRatio > 1)
        {
            if (weightRatio > 2)
                speedMod = 0;
            else
                speedMod = 2 - (inventory.weight / characterSheet.carryWeight);
        }

        return speedMod;
    }
    /// <summary>
    /// Modifies motion using fixed time based on 60fps
    /// Always use this!  Never use motionController.Move directly!
    /// </summary>
    /// <param name="direction">-1 = left/up, 1 = right/down, 0 = no motion</param>
    /// <param name="speed">direction multiplier</param>
    protected void MotionControllerMove(Vector2 direction, float speed, bool useWeightSpeedMod = true)
    {
        float weightSpeedMod = 1;
        if (useWeightSpeedMod) weightSpeedMod = GetWeightSpeedMod();

        motionController.Move(new Vector2(
            //Using variable time tied to actual fps (this old method caused motion controller to slip through colliders during low/skipped frames)
            direction.x * Time.deltaTime * speed * weightSpeedMod,
            direction.y * Time.deltaTime * speed * weightSpeedMod

        //Using fixed time based on 60fps
        //direction.x * speed * speedMod * Ponapocalypse.Constants.frameTimeRate * Time.timeScale,
        //direction.y * speed * speedMod * Ponapocalypse.Constants.frameTimeRate * Time.timeScale
        ));
    }
    protected void SetConstantMotionX(float _direction)
    {
        constantMotion.x = _direction * dir;
        //motionControllerMove(new Vector2(_direction * dir, 0), characterSheet.speedRun);
    }
    protected void SetConstantMotionY(float _direction)
    {
        constantMotion.y = _direction * dir;
        //motionControllerMove(new Vector2(0, _direction * dir), characterSheet.speedRun);
    }

    /// <summary>
    /// Handles post-death character & controller reset (usually for AI)
    /// </summary>
    /// <returns></returns>
    protected IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(bodyFadeTime);

        //Move off-screen to avoid animation flicker when resetting
        myTrans.position = new Vector3(0, -1000, 0);

        //Flip direction back to default to avoid flipping problems...
        //... if the model is spawned from the pool a 2nd time
        dir = 1;

        //Reset animation before Despawn to avoid animation errors
        character.dead = false;
    }

    //Weapon Methods
    public ActionResult CanUseThrowingWeapon()
    {
        //Are we already throwing something?
        if (throwingTimer > 0) return ActionResult.NotReady;
        //Throwing weapon equipped?
        if (throwingWeapon == null) return ActionResult.NotOwned;
        //Are there enough available?
        if (throwingWeapon.quantity < throwingWeapon.template.chargeUse) return ActionResult.NotOwned;

        return ActionResult.Success;
    }
    /// <summary>
    /// Called when animation hits the frame where object is released
    /// </summary>
    /// <param name="emitterID">The requested emitter to use</param>
    protected void ThrowingWeaponRelease(int emitterID)
    {
        //Validation
        if (!throwingWeapon.template.worldPrefab)
        {
            Debug.LogError("Throwing weapon has no world prefab!");
            return;
        }

        //Fetch emitter
        Transform emitter = character.GetEmitter(emitterID, Character.AttackType.ThrowingWeapon);

        //Spawn object
        ThrowingWeapon scriptRef = Common.ProducePrefab(
            throwingWeapon.template.worldPrefab,
            emitter.position,
            Quaternion.identity).GetComponent<ThrowingWeapon>();

        //Decrement stack
        throwingWeapon.quantity -= throwingWeapon.template.chargeUse;

        //Validation
        if (scriptRef == null)
        {
            Debug.LogError("Throwing weapon missing script!");
            return;
        }

        //Pass our stats on to the new object
        scriptRef.Init(
                this,
                throwingWeapon,
                dir,
                myTrans.position.y);

        //Cleanup if needed
        if (throwingWeapon.quantity <= 0)
        {
            //Remove from equipment list
            ChangeEquipment(throwingWeapon, false, true);

            //Remove from inventory
            inventory.RemoveItem(throwingWeapon);

            //Un-cache
            throwingWeapon = null;
        }
    }
    protected void ReloadStart()
    {
        weapon.currentState = BaseWeapon.WeaponState.Reloading;
        //weapon.playWeaponSound(weapon.template.sounds.reloadStart);

        if (weapon.template.ejectionMethod == WeaponTemplate.EjectionMethods.OnReload)
            weapon.DoEjectionEffects(character.weaponAnchors[0], dir, myTrans.position.y);
    }
    protected void ReloadMid()
    {
        int ammoToUseNow;

        switch (weapon.template.reloadMethod)
        {
            case WeaponTemplate.ReloadMethods.WholeClip:
                ammoToUseNow = weapon.ammoCountToReloadWith;
                break;
            default:
                ammoToUseNow = weapon.ammoPerReloadCycle;
                break;
        }

        if (weapon.item != null)
        {
            if ((weapon.ammoCountToReloadWith > 0) && (weapon.ammoCountToReloadWith >= ammoToUseNow) && (weapon.item.chargesLeft < weapon.item.template.chargeCount))
            {
                weapon.PlayWeaponSound(myTrans, BaseWeapon.WeaponSoundRef.reloadMid);
                weapon.ammoCountToReloadWith -= ammoToUseNow;
                weapon.item.chargesLeft += ammoToUseNow;
                AmmoChambered(ammoToUseNow);
                //SendMessageUpwards("AmmoChambered", ammoToUseNow, SendMessageOptions.RequireReceiver);
            }

            //During reloading sequences that do not add all ammo at once, check for full condition then set the end of sequence flag
            if (!((weapon.ammoCountToReloadWith > 0) && (weapon.ammoCountToReloadWith >= weapon.ammoPerReloadCycle) &&
                  (weapon.item.chargesLeft < weapon.item.template.chargeCount)))
            {
                //End reload sequence (animation trigger resets on it's own)
                weapon.currentState = BaseWeapon.WeaponState.ReloadEnd;
            }
            else
                //Continue reload sequence by firing animation trigger again
                character.Reload();
        }
    }
    protected void ReloadEnd()
    {
        weapon.currentState = BaseWeapon.WeaponState.Idle;
        //weapon.playWeaponSound(weapon.template.sounds.reloadEnd);
    }

    //Ammo Methods
    protected virtual void SetUsedAmmo(BaseItem ammoRemaing)
    {
        ammoCache = ammoRemaing;
    }
    protected void AmmoChambered(int amount)
    {
        BaseItem ammo = inventory.FindAmmo(weapon.item.template.chargeType);
        inventory.RemoveItem(ammo, amount);

        if (!inventory.items.Contains(ammo))
            ammo = null;
        else
            if (ammo.quantity == 0)
            ammo = null;

        SetUsedAmmo(ammo);
    }
    protected virtual void AutoReload()
    {
        if(allowAutoReload) Reload();
    }

    //Ability Methods
    protected void ExecuteAbility(Transform emitter)
    {
        //Spawn payload
        GameObject payload = Common.ProducePrefab(
            activatingAbility.template.payload,
            emitter.position,
            Quaternion.identity,
            activatingAbility.template.poolPayload,
            myTrans.parent);

        //Initilize payload
        payload.GetComponent<AbilityPayloadController>().Init(
            this,
            activatingAbility,
            sortingGroup.sortingOrder + 1);

        //Add status effect if present on this ability
        characterSheet.AddStatusEffect(activatingAbility.template);
    }
    protected void EndAbilitySequence()
    {
        activatingAbility = null;
    }
    protected void LearnAbility(BaseItem book)
    {
        if (characterSheet.LearnAbility(book))
            inventory.RemoveItem(book, 1);
    }

    //Collisions
    string GotItems(string sItemList)
    {
        //Update ammo HUD if we picked up ammo for the current weapon
        if ((weapon.item != null) && (ammoCache == null))
            ammoCache = inventory.FindAmmo(weapon.item.template.chargeType);

        //Fire event
        if (OnGotItems != null) OnGotItems(this, sItemList);

        return sItemList;
    }
    public virtual string OnGetLoot(Inventory loot)
    {
        //Take items
        return GotItems(loot.TransferTo(inventory));
    }
    public virtual string OnGetLoot(InventoryManifest loot)
    {
        //Take items
        return GotItems(inventory.TransferFrom(loot));
    }
    protected virtual void OnStatEffectAreaEntered(IStatusEffectSource effect)
    {
        if (initialized)
            characterSheet.AddStatusEffect(effect);
        else
            StartCoroutine(AddStatusEffectWhenInitialized(effect));
    }
    protected virtual void OnStatEffectAreaExited(IStatusEffectSource effect)
    {
        characterSheet.RemoveStatusEffect(effect);
    }
    IEnumerator AddStatusEffectWhenInitialized(IStatusEffectSource source)
    {
        while (!initialized)
            yield return new WaitForEndOfFrame();

        characterSheet.AddStatusEffect(source);
    }

    //Interactable Object Handling
    InteractableObject GetFirstInteractableObjectInRange()
    {
        if (interactableObjects.Count > 0)
            return interactableObjects[0];
        else
            return null;
    }
    public bool IsAnyInteractableObjectInRange()
    {
        return interactableObjects.Count > 0;
    }
    public bool ActivateObjectQuick(bool includeLoot)
    {
        //Start with a default object
        InteractableObject iObj = GetFirstInteractableObjectInRange();

        //Prioritize loot bags over portals
        LootPickup loot = FindNearbyLootBag();
        if (includeLoot && (loot != null))
            iObj = loot.interactableObject;

        //Was an object found?
        if (iObj)
        {
            //Ineract with the object
            iObj.InteractQuick(this);

            //If the interactive object gets disabled when used, remove it from our list
            if (!iObj.gameObject.activeInHierarchy)
                HandleInteractableObject(iObj, false);

            return true;
        }

        return false;
    }
    public bool ActivateObjectLongStart()
    {
        interacableObjectInUse = GetFirstInteractableObjectInRange();
        if ((!character.interacting) && (interacableObjectInUse != null))
        {
            //Notify the object that we wish to use it
            bool itWorked = interacableObjectInUse.InteractLongStart(this);

            //Has the object consented to being used?
            if (itWorked)
            {
                //Update the character animation
                character.interacting = true;
            }

            return itWorked;
        }

        return false;
    }
    public void ActivateObjectLongStop()
    {
        if (interacableObjectInUse != null)
        {
            //Notify the object that we stopped using it
            interacableObjectInUse.InteractLongStop(this);

            //Update the character animation
            character.interacting = false;

            //If the interactive object gets disabled when used, remove it from our list
            if (!interacableObjectInUse.gameObject.activeInHierarchy)
                interactableObjects.Remove(interacableObjectInUse);
        }

        interacableObjectInUse = null;
    }
    void HandleInteractableObject(InteractableObject iObj, bool entered)
    {
        if (entered)
        {
            //Add to tracking list
            if (!interactableObjects.Contains(iObj))
                interactableObjects.Add(iObj);
        }
        else
        {
            //Remove from tracking list
            interactableObjects.Remove(iObj);
            if ((nearestLootPickup != null) && (iObj == nearestLootPickup.interactableObject))
                nearestLootPickup = null;
        }

        //Fire event
        if (OnInteractableObjectProximityChange != null)
            OnInteractableObjectProximityChange(this, iObj, entered);
    }
    void HandleNPC(Collider2D collider, bool touched)
    {
        NPCController npc = collider.gameObject.GetComponent<NPCController>();
        if (npc == null)
        {
            Debug.LogWarning("Touched something on the NPC Layer, but it has no NPCController");
            return;
        }
        
        //Fire event
        if(OnNearestNPCChanged != null)
            OnNearestNPCChanged(touched ? npc : null);
    }

    //Collider Triggers
    void OnTriggerEnter2D(Collider2D collider) { HandleTrigger2D(collider, TriggerType.Enter); }
    void OnTriggerExit2D(Collider2D collider) { HandleTrigger2D(collider, TriggerType.Exit); }
    void HandleTrigger2D(Collider2D collider, TriggerType triggerType)
    {
        //Don't bother checking for non-interacting entities
        if (!canInteract) return;

        //Only use Enter/Exit types
        if ((triggerType != TriggerType.Enter) && (triggerType != TriggerType.Exit)) return;

        switch (collider.gameObject.layer)
        {
            case layerInteractable:
                HandleInteractableObject(
                    collider.GetComponent<InteractableObject>(),
                    triggerType == TriggerType.Enter);
                return;
            case layerNPC:
                HandleNPC(
                    collider,
                    triggerType == TriggerType.Enter);
                break;
        }
    }

    //Camera
    public void FocusCameraOnMe(bool cinematicMode = false)
    {
        CameraController.instance.FollowTarget(this, cinematicMode);
    }
    public void AddCameraShake(float magnitude)
    {
        CameraController.instance.Shake(
            magnitude,
            myTrans.position);
    }

    //Animation State Event Handling
    protected void StandardAnimationStateEnter(StateMachineParameters parameters)
    {
        //Consume stamina
        if (parameters.staminaCost != 0)
            if (!characterSheet.AP.Consume(parameters.staminaCost))
                Debug.LogWarning("Animation failed to consume AP!");

        //Set animation motion multiplier
        animationMotionMultiplier = parameters.motionMultiplier;

        //Set canAct flag
        canAct = parameters.canAct;

        //Special handling for certain animation types
        switch (parameters.animationType)
        {
            case AnimationType.Attack:
                //Store parameters for later damage application
                attackStateParameters = parameters;

                //Reset charging timer
                if (parameters.charging)
                    chargingTimer = 0;

                //Set weapon state
                if (weapon != null)
                    weapon.currentState = BaseWeapon.WeaponState.Firing;

                break;
            case AnimationType.Reload:
            case AnimationType.ReloadStart:
                ReloadStart();
                break;
            case AnimationType.ReloadEnd:
                ReloadEnd();
                break;
        }
    }
    protected void StandardAnimationStateExit(StateMachineParameters parameters)
    {
        //Special handling for certain animation types
        switch (parameters.animationType)
        {
            case AnimationType.Attack:
                if (weapon != null)
                    weapon.currentState = BaseWeapon.WeaponState.Idle;
                break;

            case AnimationType.Reload:
                ReloadEnd();
                break;
        }
    }

    //Visuals
    public virtual void DoHitEffects(Vector3 position, GameObject prefab)
    {
        if (hitEffectTimer <= 0)
        {
            hitEffectTimer = hitEffectDelay;

            if (prefab)
                Common.ProducePrefab(
                    prefab,
                    position + new Vector3(0, 0, -1),
                    Quaternion.identity,
                    true);
        }
    }

    //Sound
    public void PlaySound(CharacterSoundsTemplate.SoundKeys soundKey, bool force = false)
    {
        character.PlaySound(soundKey, force);
    }
    public virtual void PlayWeaponSound(int sound)
    {
        weapon.PlayWeaponSound(
            myTrans,
            (BaseWeapon.WeaponSoundRef)sound);
    }

    /// <summary>
    /// Am I about to hit myself or my teammates?
    /// </summary>
    /// <param name="collider">The collider being identified</param>
    /// <returns></returns>
    public bool IsHitBoxMineOrTeams(Collider collider)
    {
        int theirTeam = 0;
        HitBox hitBox = collider.GetComponent<HitBox>();
        if (hitBox) theirTeam = hitBox.team;

        return 
            hitBoxes.Contains(collider) ||
            ((theirTeam != 0) && (theirTeam == team));
    }
    void ToggleHitBoxes(bool turnOn)
    {
        if(hitBoxComps != null)
            foreach (HitBox hb in hitBoxComps)
                hb.gameObject.SetActive(turnOn);
    }
    /// <summary>
    /// Updates timers that should happen regardless of the actor's state (dead, unintialized, etc)
    /// </summary>
    protected virtual void UpdateTimers()
    {
        //Hit effect timer
        if (hitEffectTimer > 0) hitEffectTimer -= Time.deltaTime;
        //Rub timer
        if (rubbingTimer > 0) rubbingTimer -= Time.deltaTime;
        //Sneak timer
        if (sneakCooldownTimer > 0) sneakCooldownTimer -= Time.deltaTime;
        //Throwing timer
        if (throwingTimer > 0) throwingTimer -= Time.deltaTime;
        //Weapon timer
        if (weaponTimer > 0) weaponTimer -= Time.deltaTime;
    }
    /// <summary>
    /// Updates timers only if this actor is initialized and alive
    /// </summary>
    protected virtual void UpdateActiveTimers()
    {
        //Charging Timer
        if (character && character.charging) chargingTimer += Time.deltaTime;
        //Weapon timer
        if (weaponTimer > 0) weaponTimer -= Time.deltaTime;
        //Battle Timer
        if (battleTimeoutTimer > 0)
            battleTimeoutTimer -= Time.deltaTime;
        else inBattle = false;
        //AP Regen timer
        if (apUsedTimer > 0) apUsedTimer -= Time.deltaTime;
        //Propagate realtime stats
        characterSheet.Tick(Time.deltaTime, apUsedTimer <= 0);
        //Status Effect Timer
        if (statusEffectTimer > 0) statusEffectTimer -= Time.deltaTime;
        else
        {
            statusEffectTimer = statusEffectHandleTime;
            characterSheet.HandleStatusEffects();
        }
    }
    #endregion

    //Editor Methods
    void OnDrawGizmosSelected()
    {
        if (drawHitcheckGizmos &&
          ((lastMeleeHitExtents != Vector3.zero) & (lastMeleeHitPosition != Vector3.zero)))
        {
            Gizmos.DrawCube(lastMeleeHitPosition, lastMeleeHitExtents);
        }
    }
}