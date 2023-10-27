using UnityEngine;
using RPGData;
using Variables;

public class PlayerController : ActorController
{
    //Constants
    const float minPainShakeMagnitude = 2f;
    const float useButtonHeldTime = 0.25f;

    //Inspector Properties
    [Header("Player-Specific Stuff")]
    [SerializeField] AudioGroupTemplate levelUpSound;
    [SerializeField] Vector2 debugMoveTest;

    //Private Properties
    //float testEquipArmor = 0f;
    //float testEquipWeapon = 0f;
    float doubleTapTimer = 0;
    float useButtonTimer = 0;
    NPCController npcInRange = null;
    LootPickup lootTransfer = null;
    
    //Dialogue Methods
    void EnterNPCRange()
    {
        GameDatabase.sGameSettings.FloatSomeText(
            "Press 'E'\nTo interact", 
            Color.white,
            npcInRange.transform);
    }
    void ExitNPCRange()
    {

    }
    protected override void NearestNPCChanged(NPCController npc)
    {
        base.NearestNPCChanged(npc);

        if (npcInRange != null) ExitNPCRange();
        npcInRange = npc;
        if (npcInRange != null) EnterNPCRange();
    }
    bool InteractWithNPC()
    {
        if (npcInRange != null)
        {
            npcInRange.Interact(myTrans);
            return true;
        }

        return false;
    }
    
    //Player-specific initialization methods
#if UNITY_EDITOR
    protected override void Awake()
    {
#if UNITY_EDITOR
        //Play-testing / problem fixing
        if (!LoadingManager.awoken)
        {
            //Setup input
            InputManager.LoadConfig();
            Debug.Log("No LoadingManager - Initiating Test Mode");
        }
#endif
        base.Awake();
    }
#endif
    protected override void Initialize()
    {
        base.Initialize();

        //Register myself as the local player
        if (GameDatabase.localPlayer)
            Debug.LogWarning("Another player is already in the scene, this one will now take precedence");
        GameDatabase.localPlayer = this;

        //Start the camera following me
        myActor.FocusCameraOnMe();

        //Subscribe to events if available
        GameDatabase.sPlayerData.quickslotData.OnWeaponSlotChanged += WeaponQuickSlotChanged;
        GameDatabase.sPlayerData.quickslotData.OnActiveWeaponSlotChanged += ActiveWeaponQuickSlotChanged;

        GameDatabase.sPlayerData.quickslotData.OnThrowableSlotChanged += ThrowableQuickSlotChanged;
        GameDatabase.sPlayerData.quickslotData.OnActiveThrowableSlotChanged += ActiveThrowableQuickSlotChanged;

        myActor.allowAutoReload = GameDatabase.sGameSettings.autoReload;
    }

    #region Event Handlers
    protected override void Died(Actor actor)
    {
        base.Died(actor);

        //Unequip weapon
        myActor.ChangeEquipment(myActor.weapon.item, false, true);
        GUI_Common.instance.OnDead();
    }
    protected override void GotItems(Actor actor, string newItemList)
    {
        //Make floating text to show what was taken
        GameDatabase.sGameSettings.FloatSomeText(
            newItemList, 
            Color.white, 
            myTrans.position + (Vector3)myActor.floatingObjectOffset);
    }
    protected override void Hit(Actor actor, HitData hit)
    {
        //Do base class stuff
        base.Hit(actor, hit);

        //Only do this stuff if we were originally alive before the hit
        if (!myActor.dead)
        {
            //Do player-specific stuff
            if (hit.damageResult.value > 0.5f)
            {
                //Camera shake (TODO: Change to logarithmic falloff)
                float shakeMagnitude = minPainShakeMagnitude +
                    ((hit.damageResult.value / myActor.characterSheet.HP.valueModded) *
                     (CameraController.maxShakeMagnitude - minPainShakeMagnitude));
                CameraController.instance.Shake(shakeMagnitude, Vector3.zero);
            }
        }
    }
    protected override void RoomChanged(Actor actor, Room newRoom)
    {
        base.RoomChanged(actor, newRoom);

        //If this is the local player, setup local stuff
        if (newRoom != null)
        {
            //Set up post-processing lighting
            CameraController.instance.SetPostProcessingProfile(newRoom.roomTemplate.postProcessingProfile);

            if (selfLight) selfLight.color = newRoom.roomTemplate.personalLightColor;

            //Setup music
            newRoom.PlayNormalMusic();
        }
    }
    protected override void InteractableObjectProximityChange(Actor actor, InteractableObject iObject, bool entered)
    {
        base.InteractableObjectProximityChange(actor, iObject, entered);

        //Only need further code if this is an entered event
        if (!entered) return;
        
        //Should this object automatically interact on touch?
        if (iObject.automatic)
        {
            iObject.InteractQuick(myActor);
            return;
        }
        else
        {
            iObject.InteractTouch(myActor);
        }
    }
    #endregion

    //UI
    protected void ShowExhausted()
    {
        //Only show alerts from the local player entity
        //if (this == GameManager.instance.GetLocalPlayer())
            Alerter.ShowMessage("You are exhausted!");
    }
    public void InventoryTransferEnded()
    {
        //If transferring from a regular lootable, notify that object
        if (lootTransfer) lootTransfer.OnLooted();
    }
    public Actor.ActionResult ClickItem(BaseItem item, SubmitState state)
    {
        Actor.ActionResult result = myActor.UseItem(item, state);
        if (result == Actor.ActionResult.Success)
        {
            //Play GUI sound
            AudioManager.instance.Play(item.template.equipSound);

            //Set quickslot if applicable
            switch (item.template.itemType)
            {
                case ItemTypes.Consumable:
                    //Store change
                    GameDatabase.sPlayerData.quickslotData.SetActiveItemQuickSlot(
                        item.template.itemType,
                        item.template);
                    break;
                case ItemTypes.Weapon:
                    //Store change
                    GameDatabase.sPlayerData.quickslotData.SetActiveItemQuickSlot(
                        item.template.itemType,
                        item.template);
                    break;
                case ItemTypes.Throwable:
                    //Store change
                    GameDatabase.sPlayerData.quickslotData.SetActiveItemQuickSlot(
                        item.template.itemType,
                        item.template);
                    break;
            }

            //Reload quickslot gui
            GUI_Common.instance.characterSheetGUI.quickslotManager.LoadFromData();
        }

        return result;
    }

    #region Input Handlers
    //Sub handlers
    void HandleQuickslots()
    {
        //Weapon cycling
        if (!myActor.busy && InputManager.weaponQuickslot.down())
        {
            GameDatabase.sPlayerData.quickslotData.CycleActiveWeaponSlot();
            SwitchWeapon(GameDatabase.sPlayerData.quickslotData.activeWeaponRef, false);
        }

        //Throwable cycling
        if(InputManager.throwableQuickslot.down())
        {
            GameDatabase.sPlayerData.quickslotData.CycleActiveThrowableSlot();
            SwitchThrowable(GameDatabase.sPlayerData.quickslotData.activeThrowableRef);
        }

        //Consumable cycling
        if (InputManager.consumableQuickslot.down())
        {
            GameDatabase.sPlayerData.quickslotData.CycleActiveConsumableSlot();
        }

        //Ability cycling
        if (InputManager.abilityQuickslot.down())
        {
            GameDatabase.sPlayerData.quickslotData.CycleActiveAbilitySlot();
        }
    }
    void HandleReloadButton()
    {
        if(InputManager.reload.down())
            myActor.Reload();
    }
    void HandleAttackButtons()
    {
        if(InputManager.firePrimary.down())
        {
            Actor.ActionResult result = myActor.Attack();
            if (result == Actor.ActionResult.NotEnoughAP)
                ShowExhausted();
        }

        /*
        //Can't attack while staggering or in menus/conversations
        if ((myActor.canAct) && (!HUD_Connector.instance.allowPlayerControl))
        {
            bool automaticWeapon = ((myActor.weapon.item != null) && (myActor.weapon.template.fireMode == WeaponTemplate.FireModes.Auto));
            character.attackHeld = InputManager.firePrimary.held();

            if (
                //Button pressed - Manual fire weapon
                (InputManager.firePrimary.down()) ||
                //Button held down - automatic weapon
                ((automaticWeapon) && (character.attackHeld))
               )
            {
                Actor.ActionResult result = myActor.Attack();
                if (result == Actor.ActionResult.NotEnoughAP)
                    ShowExhausted();
            }
        }
        */
    }
    void HandleInteractionHeld()
    {
        useButtonTimer += Time.deltaTime;

        if (useButtonTimer <= useButtonHeldTime) return;
        if (myActor.interacableObjectInUse) return;
        if (!myActor.IsAnyInteractableObjectInRange()) return;
        
        //Prioritize loot bags
        lootTransfer = myActor.FindNearbyLootBag();
        if (lootTransfer != null)
        {
            //Open inventory swap screen
            GUI_Common.instance.OnTransfer(
                true,
                this,
                lootTransfer.inventory,
                lootTransfer.displayName);

            //Reset timer
            useButtonTimer = 0f;
        }
        //Handle other object types
        else
        {
            if (myActor.ActivateObjectLongStart())
            {
                //Reset timer
                useButtonTimer = 0;
            }
        }
    }
    void HandleInteractionInput()
    {
        //Use button first pressed
        //if (InputManager.interact.down())
        //Nothing yet

        //Use button held
        if (InputManager.interact.held())
            HandleInteractionHeld();

        //Use button released
        if (InputManager.interact.up())
        {
            //Interact with object if in range
            if (myActor.IsAnyInteractableObjectInRange())
            {
                if (useButtonTimer > useButtonHeldTime)
                {
                    //Stop using whatever we were using
                    myActor.ActivateObjectLongStop();
                }
                else
                {
                    //This was just a button tap, do a quick use
                    myActor.ActivateObjectQuick(true);
                }
            }
            else
            {
                //Interact with NPC if in range
                if (npcInRange != null)
                    InteractWithNPC();
            }

            //Reset timer
            useButtonTimer = 0;
        }
    }
    void HandleThrowingInput()
    {
        if (InputManager.useThrowingWeapon.down())
            myActor.UseThrowingWeapon();
    }
    void HandleConsumableButton()
    {
        if (InputManager.useConsumable.down())
            myActor.UseItem(
                GameDatabase.sPlayerData.quickslotData.activeConsumableRef,
                SubmitState.Normal);
    }
    void HandleAbilityButton()
    {
        if(InputManager.activateAbility.down())
        {
            BaseAbility selectedAbility = GameDatabase.sPlayerData.quickslotData.activeAbilityRef;
            if (selectedAbility != null)
                ActivateAbility(selectedAbility);
        }
    }
    void HandleDashButton()
    {
        if (!myActor.sneaking && InputManager.dash.down())
        {
            if (myActor.Dash() == Actor.ActionResult.NotEnoughAP)
                ShowExhausted();
        }
    }
    void HandleShowLogButton()
    {
        if (InputManager.showLog.up())
            GUI_Log.Toggle();
    }
    void SwitchWeapon(BaseItem nextItem, bool overrideBusy)
    {
        if (nextItem != null)
        {
            //Equip different weapon?
            if (myActor.weapon.item != nextItem)
                myActor.ChangeEquipment(
                    nextItem, 
                    true, 
                    overrideBusy);
        }
        else
        {
            //Unequip weapon?
            if (myActor.weapon.item != null)
                myActor.ChangeEquipment(
                    myActor.weapon.item, 
                    false, 
                    overrideBusy);
        }
    }
    void SwitchThrowable(BaseItem nextItem)
    {
        if (nextItem != null)
        {
            if (myActor.throwingWeapon != nextItem)
                myActor.ChangeEquipment(
                    nextItem, 
                    true, 
                    true);
        }
        else
        {
            if (myActor.throwingWeapon != null)
                myActor.ChangeEquipment(
                    myActor.throwingWeapon, 
                    false, 
                    true);
        }
    }
    void ActiveWeaponQuickSlotChanged(int slotID)
    {
        SwitchWeapon(GameDatabase.sPlayerData.quickslotData.activeWeaponRef, true);
    }
    void WeaponQuickSlotChanged(int slotID, ItemTemplate item)
    {
        if (slotID == GameDatabase.sPlayerData.quickslotData.activeWeaponSlot)
            ActiveWeaponQuickSlotChanged(slotID);
    }
    void ActiveThrowableQuickSlotChanged(int slotID)
    {
        SwitchWeapon(GameDatabase.sPlayerData.quickslotData.activeThrowableRef, true);
    }
    void ThrowableQuickSlotChanged(int slotID, ItemTemplate item)
    {
        if (slotID == GameDatabase.sPlayerData.quickslotData.activeThrowableSlot)
            ActiveThrowableQuickSlotChanged(slotID);
    }

    //Main handler
    void HandleDebugInput(ref float hor, ref float ver)
    {
#if UNITY_EDITOR
        //Debuging stuff
        if ((hor == 0) && (ver == 0))
        {
            hor = debugMoveTest.x;
            ver = debugMoveTest.y;
        }

        /*
        //Armor test
        if (Input.GetKeyDown(KeyCode.KeypadPlus) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            testEquipArmor += 1f;
            Debug.Log("Armor = " + testEquipArmor);
            character.SetGenericAnimKey("Armor", testEquipArmor);
        } else
        if (Input.GetKeyDown(KeyCode.KeypadMinus) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            testEquipArmor -= 1f;
            Debug.Log("Armor = " + testEquipArmor);
            character.SetGenericAnimKey("Armor", testEquipArmor);
        }
        else
        //Weapon test
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            testEquipWeapon += 1f;
            Debug.Log("Weapon = " + testEquipWeapon);
            character.SetGenericAnimKey("Weapon", testEquipWeapon);
        }
        else
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            testEquipWeapon -= 1f;
            Debug.Log("Weapon = " + testEquipWeapon);
            character.SetGenericAnimKey("Weapon", testEquipWeapon);
        }
        */
#endif
    }
    void HandlePlayerInput()
    {
        //bool run = true;
        float hor = 0f, ver = 0f;

        if (GUIManager.instance.allowPlayerInput)
        {
            HandleInteractionInput();

            if (myActor.canAct)
            {
                myActor.sneaking = myActor.canSneak && InputManager.sneak.held();
                hor = InputManager.moveLeft.axisValue();
                ver = InputManager.moveUp.axisValue();

                HandleDebugInput(ref hor, ref ver);
                HandleReloadButton();
                HandleQuickslots();
                HandleThrowingInput();
                HandleConsumableButton();
                HandleDashButton();
                HandleAbilityButton();
            }

            HandleAttackButtons();
        }

        myActor.Move(hor, ver, true);
    }
    #endregion
    
    #region Monobehavior Methods
    void Update()
    {
        //Propagate timers
        if (doubleTapTimer > 0)
            doubleTapTimer -= Time.unscaledDeltaTime;

        //Stuff to do only after actor is initialized
        if (myActor && myActor.initialized)
        {
            if (!myActor.dead)
                HandlePlayerInput();

            HandleShowLogButton();
        }
    }
    void OnDestroy()
    {
        //Attempt at helping the Garbage Collector do a better job
        npcInRange = null;

        GameDatabase.sPlayerData.quickslotData.OnActiveWeaponSlotChanged -= ActiveWeaponQuickSlotChanged;
        GameDatabase.sPlayerData.quickslotData.OnActiveThrowableSlotChanged -= ActiveThrowableQuickSlotChanged;
    }
    protected override void SetControlTarget(Actor target)
    {
        if (myActor) myActor.controlledByPlayer = false;

        base.SetControlTarget(target);

        myActor.controlledByPlayer = true;
    }
    #endregion

    #region Other Methods
    protected override Actor.ActionResult ActivateAbility(BaseAbility ability)
    {
        Actor.ActionResult result = base.ActivateAbility(ability);

        //Alert the player if needed
        switch (result)
        {
            case Actor.ActionResult.NotEnoughMP:
                Alerter.ShowMessage("Not enough MP!");
                break;
            case Actor.ActionResult.NotEnoughAP:
                Alerter.ShowMessage("Not enough AP!");
                break;
            case Actor.ActionResult.NotValid:
                Debug.LogWarning("My actor said this ability is invalid: " + ability.template.displayName);
                break;
            case Actor.ActionResult.NotOwned:
                Debug.LogWarning("My actor does not own ability " + ability.template.displayName);
                break;
        }

        return result;
    }
    public void PrepareForReturnToMap()
    {
        //If the player died, restore them before transition
        if (myActor.dead) myActor.Resurrect();
    }
    #endregion
}