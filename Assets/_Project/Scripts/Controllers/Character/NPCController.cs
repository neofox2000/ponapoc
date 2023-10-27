using UnityEngine;
using System;
using Events;
using Variables;

public class NPCController : ActorController
{
    #region Properties
    [Tooltip("Unique ID used to store persistent data")]
    public int persistentID;
    [SerializeField] Vector2 interactRange = new Vector2(6f, 1f);
    [SerializeField] ActorControllerVariable barterTarget;
    [SerializeField] GameEvent startBarterEvent;
    [SerializeField] BooleanVariable barterResult;

    BoxCollider2D interactionCollider;
    //Actor player;

    float _haggleAttemptsLeft = 0;
    public float tradingRate { get; set; } = -1;
    #endregion

    #region Conversation Methods
    public void Interact(Transform initiator)
    {
        //If barter is initiated, this NPC Controller will be the target
        barterTarget.SetValue(this);

        if (!myActor.conversationFlowchart)
        {
            Debug.LogError(name + ": No conversation flowchart assigned to myActor - cannot start convo");
            return;
        }

        //Face initiator
        myActor.dir = (initiator.position.x < myTrans.position.x) ? -1 : 1;

        //Greeting
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcGreeting, false);

        //Focus camera on speaker
        //FocusCameraOnMe(true);

        //Start conversation
        myActor.conversationFlowchart.SendFungusMessage(
            DialogueMessages.StartConversation);
    }
    public void BarterStart()
    {
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcBarterStarted);
    }
    public override void BarterEnd()
    {
        //Only respond if this is the correct NPC being interracted with
        if (barterTarget.Value != this) return;

        //myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcFairwell);
        if(barterResult.value)
            myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcBarterAccepted);
        else
            myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcBarterCanceled);

        //Start back up the dialogue
        Dialog.SendBarterResult(
            myActor.conversationFlowchart,
            barterResult.value ? BarterResult.Accepted : BarterResult.Cancelled);
    }
    public void ConversationEnd()
    {

    }

    public void BarterNoDeal()
    {
        //Barter Offer Rejected
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcBarterRejected);

        //NPC response
        Dialog.SendBarterResult(
            myActor.conversationFlowchart,
            BarterResult.Failed);
    }
    public void BarterHaggleSuccess()
    {
        //Haggle Succeeded
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcHaggleSuccess);

        //NPC response
        Dialog.SendHaggleResult(
            myActor.conversationFlowchart,
            HaggleResult.Success);
    }
    public void BarterHaggleFailure()
    {
        //Haggle Failed
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcHaggleFailure);

        //NPC response
        Dialog.SendHaggleResult(
            myActor.conversationFlowchart,
            HaggleResult.Fail);
    }
    public void HaggleExhausted()
    {
        //Haggle Exhausted
        myActor.PlaySound(CharacterSoundsTemplate.SoundKeys.npcHaggleExhausted);

        //NPC response
        Dialog.SendHaggleResult(
            myActor.conversationFlowchart,
            HaggleResult.Exhausted);
    }
    public float GetRemainingHaggleAttempts(float targetAttemptsMod = 0f)
    {
        return _haggleAttemptsLeft + targetAttemptsMod;
    }
    public void SetRemainingHaggleAttempts(float newValue, float targetAttemptsMod = 0f)
    {
        _haggleAttemptsLeft = newValue - targetAttemptsMod;
    }
    #endregion

    //Override Methods
    protected override void GenerateInventory()
    {
        var savedData = GameDatabase.sPlayerData.npcData.Fetch(persistentID);

        if (savedData == null)
            base.GenerateInventory();
        else
            savedData.Unpack(this);
    }
    protected override void Initialize()
    {
        base.Initialize();

        interactionCollider = gameObject.AddComponent<BoxCollider2D>();
        //interactionCollider.radius = interactRadius;
        interactionCollider.size = interactRange;
        //interactionCollider.center = new Vector3(0, 1, 0);
        interactionCollider.isTrigger = true;

        //Set interaction
        if (interactionCollider)
            interactionCollider.enabled = myActor.isInteractable;

        //Track persistent data
        GameDatabase.metaGame.TrackNPC(this);
    }
}