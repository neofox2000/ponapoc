using UnityEngine;
using RPGData;
using System.Collections.Generic;

public class AIController : ActorController //Actor
{
    #region Constants & Enums
    const float minThinkTime = 0.1f;
    const float seekTime = 5f;
    const float stuckTime = 1f;
    const float abilityChoosingTime = 3f;
    const float defaultMaxUnarmedAttackRange = 1f;
    const float defaultMinUnarmedAttackRange = 0.2f;
    const float minVerticalDistance = 0.1f;
    const float minHorizontalDistance = 0.1f;
    const float fearDistance = 15f;

    public enum AIStates { Idle, Seek, Attack, Fear, TakeCover, Staggering }
    #endregion

    #region Properties
    bool _inMotion = false;
    
    //Used to avoid unneccessary overhead from checking ability list
    bool hasOffensiveAbilities = true;
    
    //Timers
    float
        thinkTimer = 0f,
        idleBarkTimer = 0f,
        stuckTimer = 0f,
        abilityChoosingTimer = 0f,
        seekTimer = 0f;
    float minAttackRange, maxAttackRange;
    AIStates _currentState = AIStates.Idle;
    AIStates currentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState != value)
            {
                switch (value)
                {
                    case AIStates.Attack:
                        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.aiStateAttack, true);
                        break;
                    case AIStates.Seek:
                        seekTimer = seekTime;
                        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.aiStateSeek, true);
                        break;
                    case AIStates.Idle:
                        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.aiStateIdle, true);
                        break;
                }

                _currentState = value;
            }
        }
    }
    Vector3 currentMoveToPosition;
    Vector3 lastMoveToPosition;

    //Target cache
    Actor currentTarget = null;
    Transform currentTargetTransform = null;
    
    /// <summary>
    /// The ability that this mob feels like using
    /// It is assigned periodically during the think step
    /// </summary>
    BaseAbility chosenAbility = null;

    //Flags if this AI is supposed to be moving (helps determine if stuck)
    bool inMotion
    {
        get { return _inMotion; }
        set
        {
            _inMotion = value;
            if (!value)
                stuckTimer = 0;
        }
    }

    //Delay between AI actions
    float thinkTime = 0.5f;

    //How likely AI is to walk about in idle states
    float roamChance = 0.5f;

    //How far the AI will walk when roaming
    float roamDistance = 1f;

    bool hasAbilities { get { return myActor.characterSheet.abilities.Count > 0; } }
    float charSideWidth
    {
        get
        {
            return currentTarget.dir == 1 ?
                myActor.characterWidthBySide.y :
                myActor.characterWidthBySide.x;
        }
    }

    #endregion

    #region Methods
    void HandleMovement()
    {
        if (inMotion)
        {
            //Have we reached the horizontal target?
            float magX = ReachedTargetX() ? 0 : 1;
            //Have we reached the vertical target?
            float magY = ReachedTargetY() ? 0 : 1;

            //Should we be running or walking?
            bool run = 
                myActor.canRun && 
                ((currentState == AIStates.Seek) || 
                 (currentState == AIStates.Fear));

            //Move me
            myActor.Move(
                currentMoveToPosition.x > myTrans.position.x ? magX : -magX,
                currentMoveToPosition.y > myTrans.position.y ? magY : -magY,
                run);

            //Used for stuck checks
            float minDistance = (myActor.speed * Time.deltaTime) * 4;

            //if (lastMovingPosition != myTrans.position)
            if (minDistance < Vector3.Distance(lastMoveToPosition, myTrans.position))
            {
                //Not stuck, reset timer & update last position
                lastMoveToPosition = myTrans.position;
                stuckTimer = 0;
            }
            else
                //Possibly stuck, increment timer
                stuckTimer += Time.deltaTime;
        }
        else
        {
            myActor.Move(0f, 0f, false);
        }
    }
    void HandleBarks()
    {
        if (idleBarkTimer <= 0)
        {
            idleBarkTimer = Random.Range(10f, 20f);
            if( (currentTarget != null) && 
                (currentTarget.currentRoom == myActor.currentRoom) && 
                (Mathf.Abs(currentTarget.transform.position.x - myTrans.position.x) < 30f)
              )
                Dialog.BarkIdle(
                    myActor.conversationFlowchart.GetComponent<Fungus.Flowchart>(),
                    myTrans);
        }
    }
    void HandleTimers()
    {
        if (idleBarkTimer > 0)
            idleBarkTimer -= Time.deltaTime;

        if (abilityChoosingTimer > 0)
            abilityChoosingTimer -= Time.deltaTime;

        if (seekTimer > 0)
            seekTimer -= Time.deltaTime;
    }

    //Main Action Methods
    void ActIdle()
    {
        bool doIdle = true;
        if (currentTarget)
        {
            if (myActor.inBattle || InAggroRange())
            {
                doIdle = false;
                if (InAnyMaxRange(currentTargetTransform.position))
                    currentState = AIStates.Attack;
                else
                    currentState = AIStates.Seek;
            }
        }

        if (doIdle)
        {
            myActor.inBattle = false;

            if (ReachedTarget())
                inMotion = false;

            if(inMotion && IsStuck())
            {
                //Got stuck - stop and re-evaluate.
                inMotion = false;
                //Debug.Log(name + " got stuck");
                //Debug.DrawLine(myTrans.position, myTrans.position + Vector3.right + Vector3.down, Color.green, 0.2f);
            }

            if (!inMotion)
            {
                if (roamChance > Random.Range(0f, 1f))
                {
                    Vector3 newPos = myTrans.position + 
                        new Vector3(Random.Range(-10f, 10f), Random.Range(-3f, 3f));

                    float distance = Vector3.Distance(myTrans.position, newPos);

                    //Don't go too far all at once
                    if(distance > roamDistance)
                    {
                        float shaveOff = distance - roamDistance;
                        newPos = Vector3.Lerp(myTrans.position, newPos, shaveOff / distance);
                    }

                    currentMoveToPosition = newPos;

                    /*
                    //TODO: Change to using some kind of 2D navmesh system
                    float vertRange = Mathf.Abs(currentRoom.bounds.vertical.y - currentRoom.bounds.vertical.x) * 0.5f;
                    targetPosition.x =
                        Mathf.Clamp(
                            myTrans.position.x + Random.Range(-roamDistance, roamDistance),
                            currentRoom.bounds.horizontal.x, 
                            currentRoom.bounds.horizontal.y);
                    targetPosition.y = 
                        Mathf.Clamp(
                            myTrans.position.y + Random.Range(-vertRange, vertRange),
                            currentRoom.bounds.vertical.x * 0.9f,
                            currentRoom.bounds.vertical.y * 0.9f);
                    */
                    if (currentMoveToPosition == Vector3.zero)
                        Debug.Log(name + " is moving to a zero position!");

                    inMotion = true;
                }
                else
                    inMotion = false;
            }

            //handleBarks();
        }
    }
    void ActSeek()
    {
        if (currentTargetTransform && ((seekTimer > 0) || myActor.inBattle))
        {
            //Cache player's position
            Vector3 targetPos = currentTargetTransform.position;

            myActor.inBattle = true;
            bool inMaxRange = InAnyMaxRange(targetPos);
            bool inMinRange = InMinRange(targetPos, minAttackRange);
            if (inMaxRange && inMinRange)
            {
                //Start attack phase
                inMotion = false;
                currentState = AIStates.Attack;
            }
            else
            {
                //Run to optimum distance from the player
                currentMoveToPosition = CalculateOptimalRange(targetPos, inMinRange);
                inMotion = true;
            }
        }
        else
        {
            //Lost aggro, go back to idle
            inMotion = false;
            thinkTimer = minThinkTime;
            currentMoveToPosition = myTrans.position;
            currentState = AIStates.Idle;
        }
    }
    void ActAttack()
    {
        if (currentTargetTransform)
        {
            myActor.inBattle = true;
            Vector3 targetPosition = currentTargetTransform.position;

            //Try ability attack
            if (chosenAbility != null)
            {
                //Use ability
                ActivateAbility(chosenAbility);

                //Unset ability to avoid cooldown spamming
                chosenAbility = null;
            }

            //Check weapon for problems (no ammo, etc)
            if((myActor.weapon.item != null) && (myActor.weapon.template.weaponType != WeaponTemplate.WeaponTypes.Melee))
            {
                //Do we need to reload?
                if(myActor.weapon.item.chargesLeft <= 0)
                {
                    //Is reload possible?
                    if (myActor.weapon.canReload)
                    {
                        Actor.ActionResult result = myActor.Reload();

                        //Did reload work?
                        if (result != Actor.ActionResult.NotOwned)
                        {
                            //Reload failed due to no ammo - try to equip another functional weapon
                            myActor.ChangeAllEquipment(true, false, new ItemTypes[] { ItemTypes.Weapon });

                            //Allow equipment to change before attacking
                            thinkTimer = 0.5f;
                            return;
                        }
                    }
                }
            }

            //Try normal attack
            if (inMaxRange(targetPosition, maxAttackRange))
            {
                if (InMinRange(targetPosition, minAttackRange))
                {
                    if (myActor.characterSheet.AP.valueCurrent > 0)
                    {
                        //Set delay before next action
                        thinkTimer = 0.5f;

                        //Turn to face target if needed
                        if (!IsFacingTarget(targetPosition))
                            myActor.dir *= -1;

                        //Stop moving
                        inMotion = false;

                        //Attack target
                        myActor.Attack();
                    }
                    else
                    {
                        //Set delay before next action
                        thinkTimer = minThinkTime;
                        
                        //Go to fear state
                        currentState = AIStates.Fear;
                    }
                }
                else
                {
                    //Target too close, go back to seek
                    //TODO: Add a chance to switch to a melee weapon instead
                    currentState = AIStates.Seek;
                }
            }
            else
            {
                //Target out of range, go back to seek
                currentState = AIStates.Seek;
            }
        }
        //Lost target, go back to idle
        else
        {
            //Stop moving
            inMotion = false;
            
            //Reset position to avoid walking immediately after entering next state
            currentMoveToPosition = myTrans.position;

            //Switch to idle state
            currentState = AIStates.Idle;
        }
    }
    void ActFear()
    {
        if (currentTargetTransform)
        {
            if (myActor.characterSheet.AP.valueCurrent <= 20f)
            {
                //Cache
                Vector3 targetPos = currentTargetTransform.position;
                float dist = DistanceFromTarget(targetPos);

                //Are we far enough away from target?
                if (dist >= fearDistance)
                {
                    //Stop
                    inMotion = false;

                    //Wait for AP to regen
                    thinkTimer = 0.5f;
                }
                else
                {
                    //Run away from the target
                    currentMoveToPosition = new Vector3(
                        targetPos.x + (targetPos.x < myTrans.position.x ? +fearDistance : -fearDistance),
                        myTrans.position.y,
                        myTrans.position.z);

                    //GO!
                    inMotion = true;
                }
            }
            //Stamina refreshed, go get 'em!
            else
            {
                //Go back to seeking
                currentState = AIStates.Seek;
            }
        }
        //Lost target, go back to idle
        else
        {
            //Stop
            inMotion = false;
            
            //Unset position
            currentMoveToPosition = myTrans.position;
            
            //Go back to idle
            currentState = AIStates.Idle;
        }
    }
    void Act()
    {
        //Get target if we have none
        if (currentTargetTransform == null)
            GetTarget();

        //Choose an ability to be used when needed
        if (hasAbilities && (abilityChoosingTimer <= 0))
        {
            //Reset timer
            abilityChoosingTimer = abilityChoosingTime;

            //Reset chosen ability
            chosenAbility = null;

            //Select an appropriate ability if available
            if (hasOffensiveAbilities)
            {
                List<BaseAbility> validAbilities = myActor.characterSheet.GetAbilitiesNotOnCooldown(
                    new AbilityTemplate.AbilityType[] { AbilityTemplate.AbilityType.Offensive });

                if (validAbilities.Count > 0)
                    chosenAbility = validAbilities[Random.Range(0, validAbilities.Count)];
            }
        }

        //Decide what to do with target (or lack thereof)
        switch (currentState)
        {
            case AIStates.Idle:
                ActIdle();
                break;
            case AIStates.Attack:
                ActAttack();
                break;
            case AIStates.Seek:
                ActSeek();
                break;
            case AIStates.Fear:
                ActFear();
                break;
        }
    }

    //Event Handlers
    void TargetDied(Actor entity)
    {
        ClearTarget();
    }
    void TargetLeftRoom(Actor entity, Room newRoom)
    {
        ClearTarget();
    }

    //Targetting Methods
    void GetTarget()
    {
        //Make sure we're intialized properly (in a valid room)
        if (!myActor.currentRoom) return;

        //Make sure the room has fully initialized and registered all actors within it
        List<Actor> actorsInRoom = myActor.currentRoom.GetActorsInRoom();
        if (actorsInRoom.Count == 0) return;

        //Reset everything
        //ClearTarget();

        //Set defaults
        float lastDist = myActor.characterSheet.detectionRange;
        float newDist = 0f;
        Actor newTarget = null; //GameManager.instance.getLocalPlayer();

        //Find closest target
        foreach (Actor entity in actorsInRoom)
        {
            //Don't target dead things
            if (entity.dead) continue;
            //Don't target teammates
            if (myActor.team == entity.team) continue;

            //Prioritise nearest target
            newDist = DistanceFromTarget(entity.transform.position);
            if (newDist < lastDist)
            {
                lastDist = newDist;
                newTarget = entity;
            }
        }

        //Did we get a valid target?
        if (!newTarget) return;

        //Cache the transform
        currentTargetTransform = newTarget.transform;

        //Cache Entity
        currentTarget = newTarget;

        //Listen for events on target that would prompt a change of targets
        currentTarget.OnDied += TargetDied;
        currentTarget.OnChangeRoom += TargetLeftRoom;
    }
    void ClearTarget()
    {
        //Stop listening for events
        if(currentTarget)
        {
            currentTarget.OnDied -= TargetDied;
            currentTarget.OnChangeRoom -= TargetLeftRoom;
        }

        //Clear target cache
        currentTargetTransform = null;
        currentTarget = null;
    }

    //Detection Methods
    float DistanceFromTarget(Vector3 targetPosition)
    {
        return Mathf.Abs(targetPosition.x - myTrans.position.x);
    }
    float HorizontalDistanceFromTarget(Vector3 target)
    {
        return Mathf.Abs(target.x - myTrans.position.x);
    }
    float VerticalDistanceFromTarget(Vector3 target)
    {
        return Mathf.Abs(target.y - myTrans.position.y);
    }
    float CalculatedRange(float range)
    {
        return
            //Requested range
            range +
            //Character's width
            charSideWidth + 
            //Character's horizontal offset (modified by current direction)
            (myActor.controllerOffset.x * myActor.dir);
    }
    Vector3 CalculateOptimalRange(Vector3 targetPosition, bool inMinRange)
    {
        //Which side is the target on?
        float sideMod = targetPosition.x < myTrans.position.x ? 1 : -1;

        //Which range is optimal?
        float rangeToUse = inMinRange ?
            CalculatedRange(minAttackRange) + 0.2f :
            CalculatedRange(maxAttackRange) - 0.2f;

        //Return optimal range
        return new Vector3(
            targetPosition.x + (rangeToUse * sideMod),
            targetPosition.y,
            targetPosition.z);
    }
    bool ReachedTargetX()
    {
        return HorizontalDistanceFromTarget(currentMoveToPosition) < minHorizontalDistance;
    }
    bool ReachedTargetY()
    {
        return VerticalDistanceFromTarget(currentMoveToPosition) < minVerticalDistance;
    }
    bool ReachedTarget()
    {
        if (currentMoveToPosition != Vector3.zero)
        {
            return
                ReachedTargetX() &&
                ReachedTargetY();
        }

        return false;
    }
    bool IsFacingTarget(Vector3 targetPos)
    {
        return 
            (((myTrans.position.x - targetPos.x) < 0) &&  (myActor.dir > 0)) ||
            (((myTrans.position.x - targetPos.x) > 0) && (myActor.dir < 0));
    }
    bool InAnyMaxRange(Vector3 targetPosition)
    {
        return 
            //Check for ranged ability first
            ((chosenAbility != null) && (InAbilityRange(targetPosition, chosenAbility.template))) ||
            //Check for normal attack range
            (inMaxRange(targetPosition, maxAttackRange));
    }
    bool InAbilityRange(Vector3 targetPosition, AbilityTemplate ability)
    {
        return inMaxRange(targetPosition, ability.range);
    }
    bool InMinRange(Vector3 targetPosition, float range)
    {
        return
            (HorizontalDistanceFromTarget(targetPosition) > CalculatedRange(range)) &&
            (VerticalDistanceFromTarget(targetPosition) < minVerticalDistance);
    }
    bool inMaxRange(Vector3 targetPosition, float range)
    {
        return
            (HorizontalDistanceFromTarget(targetPosition) < CalculatedRange(range)) &&
            (VerticalDistanceFromTarget(targetPosition) < minVerticalDistance);
    }
    bool InAggroRange()
    {
        float dist = DistanceFromTarget(currentTargetTransform.position);
        if (currentTarget && currentTarget.sneaking)
            return dist < Mathf.Max(0.1f, myActor.characterSheet.detectionRange);
        else
            return dist < myActor.characterSheet.detectionRange;
    }
    bool IsStuck()
    {
        return stuckTimer > stuckTime;
    }

    //Override Methods
    protected override void Died(Actor actor)
    {
        base.Died(actor);

        //Drop bag of loot for player
        myActor.DropInventory();
    }
    protected void ChangedWeapon(BaseItem item)
    {
        if(item != null)
        {
            //Set Weapon range
            if (item.template.isRangedWeapon())
            {
                //Stay a little further than 0 to avoid the muzzle going past the target
                minAttackRange = item.template.range * 0.15f;

                //Get a little closer than maximum range to avoid attacking prematurely
                //And also to give the player a chance to shoot first
                maxAttackRange = item.template.range * 0.85f;
            }
            else
            {
                //Stay a little further than 0 to avoid the hit point being too far past the target
                minAttackRange = defaultMinUnarmedAttackRange;
                
                //Get a little closer than maximum range to avoid attacking prematurely
                maxAttackRange = item.template.range * 0.9f;
            }
        }
        else
        {
            //Set Unarmed range
            minAttackRange = defaultMinUnarmedAttackRange;
            maxAttackRange = defaultMaxUnarmedAttackRange;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        maxAttackRange = defaultMaxUnarmedAttackRange;

        //Cache ability use flags
        hasOffensiveAbilities = myActor.characterSheet.GetAbilitiesNotOnCooldown(
            new AbilityTemplate.AbilityType[] { AbilityTemplate.AbilityType.Offensive })
            .Count > 0;

        //Clear values from last spawn (if spawned from pool)
        abilityChoosingTimer = 0f;
        chosenAbility = null;
        ClearTarget();
    }
    private void Update()
    {
        if (GameManager.instance)
        {
            if ((!myActor.dead) && (myActor.initialized))
            {
                //Tick other timers
                HandleTimers();

                //Is it time to Act?
                if (thinkTimer < 0)
                {
                    thinkTimer = thinkTime;
                    if (myActor.canAct)
                        Act();
                }
                else
                    thinkTimer -= Time.deltaTime;

                //Calculation any motion required
                HandleMovement();
            }
        }
    }
    #endregion
}