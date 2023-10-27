using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Variables;
using RPGData;
using Events;

public class GUI_Barter : MonoBehaviour 
{
    #region Non-Methods
    const float overValueRatio = 0.1f;

    [Header("Data Links")]
    public Inventory defaultEmptyInventory;
    public ActorControllerVariable controllerRight;
    public AttributeTemplate TradeSkill;
    public BooleanVariable barterResult;
    public GameEvent barterEndedEvent;

    [Header("Widgets")]
    public Button cancelButton;
    public GUI_InventoryManager
        leftInventory,
        rightInventory,
        leftBasket,
        rightBasket;
    public Text
        valueLabel,
        leftNameLabel,
        rightNameLabel,
        leftBasketLabel,
        rightBasketLabel,
        haggleButtonLabel;

    //Private Properties
    bool _showing = false;
    bool _modified = false;
    bool npcTrading = false;
    float _valueBalance = 999f;
    float tradingRate = 50f;
    float haggleChance = 50f;
    int haggleAttemptsLeft = 0;
    CharacterSheet otherCharacterSheet;
    NPCController npcTrader;

    //Accessors
    public bool showing
    {
        get { return _showing; }
        set { 
            _showing = value;
            SendMessageUpwards("updatePauseState", SendMessageOptions.DontRequireReceiver);
        }
    }
    public bool modified
    {
        get { return _modified; }
        set { _modified = value; }
    }
    public float valueBalance
    {
        get { return _valueBalance; }
        set
        {
            if (_valueBalance != value)
            {
                _valueBalance = value;

                valueLabel.text = Mathf.Abs(_valueBalance).ToString("$#,0.00");

                //Set color
                if ((_valueBalance) < 0)
                    valueLabel.color = Color.red;
                else
                    valueLabel.color = Color.green;
            }
        }
    }
    float leftTradingRate
    {
        get { return tradingRate / 100; }
    }
    float rightTradingRate
    {
        get { return npcTrading ? 2 - leftTradingRate : leftTradingRate; }
    }
    #endregion

    #region Methods
    void UpdateDisplay()
    {
        modified = false;
        leftBasketLabel.text = "Your Offer";
        rightBasketLabel.text = "Their Offer";

        if (npcTrading && haggleAttemptsLeft > 0)
        {
            //leftBasketLabel.text = "Your Offer (" + leftRate.ToString("#,0%") + ")";
            //rightBasketLabel.text = "Their Offer (" + rightRate.ToString("#,0%") + ")";
            //haggleButtonLabel.text = "Haggle (" + Mathf.Max(0, haggleAttemptsLeft).ToString() + ") [" + haggleChance.ToString(Common.defaultIntFormat) + "%]";
            haggleButtonLabel.text = "Haggle (" + haggleChance.ToString(Common.defaultIntFormat) + "%)";
        }
        else
        {
            //leftBasketLabel.text = "Your Offer";
            //rightBasketLabel.text = "Their Offer";
            haggleButtonLabel.text = "No Haggling";
        }

        //Set balance
        valueBalance =
            (leftBasket.inventoryLink.GetTotalValue() * leftTradingRate) -
            (rightBasket.inventoryLink.GetTotalValue() * rightTradingRate);

        //Refresh UI Panels
        leftInventory.refresh = true;
        rightInventory.refresh = true;
        leftBasket.refresh = true;
        rightBasket.refresh = true;
    }
    void CalcHaggleChance()
    {
        //Haggle equation
        haggleChance = 50 + Mathf.Clamp(
            ((GameDatabase.lCharacterSheet.haggleSuccessRate /
              otherCharacterSheet.haggleSuccessRate * 100) - 100),
            -50f, 50f);
    }
    void EndBarter(bool accepted)
    {
        barterResult.value = accepted;
        barterEndedEvent.Raise();
    }
    void SetupHaggling()
    {
        //Set haggle number of chances
        if (npcTrading)
        {
            //Get remaining haggle attempts from controller
            haggleAttemptsLeft = Mathf.RoundToInt(npcTrader.GetRemainingHaggleAttempts(
                GameDatabase.lCharacterSheet.haggleAttempts));

            //Set Trading rate
            if (npcTrader.tradingRate == -1)
                tradingRate = 50f;
            else
                tradingRate = npcTrader.tradingRate;

            //Set haggle success rate
            CalcHaggleChance();
        }
        else
        {
            tradingRate = 100;
        }
    }
    void SaveHaggling()
    {
        if (npcTrading)
        {
            //Haggling is only enabled for NPCControllers so we can safely assume trader is an NPC
            NPCController npc = (NPCController)controllerRight.Value;
            npc.SetRemainingHaggleAttempts(
                haggleAttemptsLeft, 
                GameDatabase.lCharacterSheet.haggleAttempts);
            npc.tradingRate = tradingRate;
        }
    }
    void RejectTransaction()
    {
        if (npcTrading) npcTrader.BarterNoDeal();
    }
    void CompleteTransaction()
    {
        rightBasket.inventoryLink.TransferTo(leftInventory.inventoryLink);
        leftBasket.inventoryLink.TransferTo(rightInventory.inventoryLink);
        EndBarter(true);
    }
    void CancelTransaction()
    {
        rightBasket.inventoryLink.TransferTo(rightInventory.inventoryLink);
        leftBasket.inventoryLink.TransferTo(leftInventory.inventoryLink);
        EndBarter(false);
    }

    public void Activate()
    {
        //Create new things if needed
        if (!leftBasket.inventoryLink) leftBasket.inventoryLink =
                ScriptableObject.Instantiate<Inventory>(defaultEmptyInventory);
        if (!rightBasket.inventoryLink) rightBasket.inventoryLink =
                ScriptableObject.Instantiate<Inventory>(defaultEmptyInventory);

        //Cache commonly used aspects of each controller
        if (controllerRight.Value is NPCController)
        {
            npcTrading = true;
            npcTrader = (NPCController)controllerRight.Value;
        }

        otherCharacterSheet = controllerRight.Value.myActor.characterSheet;
        leftInventory.inventoryLink = GameDatabase.lInventory;
        rightInventory.inventoryLink = controllerRight.Value.myActor.inventory;

        //Setup names
        leftNameLabel.text = GameDatabase.lCharacterSheet.characterName; //+ " (" + playerCharacterSheet.GetSkill(TradeSkill).valueModded.ToString() + ")";
        rightNameLabel.text = otherCharacterSheet.characterName; //+ " (" + otherCharacterSheet.GetSkill(TradeSkill).valueModded.ToString() + ")";

        //Setup haggling stuff
        SetupHaggling();

        //Internal checks
        modified = true;
        showing = true;
        //gameObject.SetActive(true);
    }
    public void DeActivate()
    {
        //Save haggle stats
        SaveHaggling();
        showing = false;
        //gameObject.SetActive(false);
    }
    public void CancelTrade()
    {
        if ((leftBasket.inventoryLink.GetTotalValue() > 0) || (rightBasket.inventoryLink.GetTotalValue() > 0))
            GUI_YesNoBox.instance.Show("Cancel trade?", true, CancelTransaction);
        else
            CancelTransaction();
    }
    public void AcceptTrade()
    {
        float rightValue = rightBasket.inventoryLink.GetTotalValue() * rightTradingRate;
        float leftValue = leftBasket.inventoryLink.GetTotalValue() * leftTradingRate;
        if ((rightValue > 0) && (valueBalance >= 0))
        {
            if (1 - (rightValue / leftValue) > overValueRatio)
                GUI_YesNoBox.instance.Show(
                    "This is a bad deal for you.  Do you really want to do it?", 
                    true, 
                    CompleteTransaction);
            else
                CompleteTransaction();
        }
        else
        {
            RejectTransaction();
        }
    }
    public void Haggle()
    {
        //Haggling is only allowed with NPCs, so we can safely assume that controllerRight is an NPC
        if (haggleAttemptsLeft > 0)
        {
            if (haggleChance > Random.Range(0f, 100f))
            {
                tradingRate += 5;
                npcTrader.BarterHaggleSuccess();
            }
            else
            {
                tradingRate -= 5;
                npcTrader.BarterHaggleFailure();
            }

            haggleAttemptsLeft--;
            modified = true;
        }
        else
            npcTrader.HaggleExhausted();
    }
    public void ItemClicked(GUI_InventoryManager targetInventoryManager, GUI_InventoryItemRow itemRow, SubmitState state)
    {
        GUI_Common.instance.HideItemDetails();
        BaseItem theItem = (BaseItem)itemRow.sourceLink;

        //Unequip item if applicable
        if ((theItem.equipped) && (theItem.quantity == 1))
        {
            if (leftInventory.inventoryLink == targetInventoryManager.inventoryLink)
                GameDatabase.localPlayer.ClickItem(theItem, SubmitState.Normal);
        }

        //Prevent equipped items from being traded
        if ((!theItem.equipped) || (theItem.quantity > 1))
        {
            GUI_InventoryManager receiver;

            //Adding items to the left basket
            if (targetInventoryManager == leftInventory)
                receiver = leftBasket;
            else
                //Removing items from the left basket
                if (targetInventoryManager == leftBasket)
                    receiver = leftInventory;
                else
                    //Adding items to the right basket
                    if (targetInventoryManager == rightInventory)
                        receiver = rightBasket;
                    else
                        //Removing items from the right basket
                        receiver = rightInventory;

            //Quantity
            int qty;
            switch (state)
            {
                case SubmitState.Negative: qty = 5; break;
                case SubmitState.Positive: qty = theItem.quantity; break;
                default: qty = 1; break;
            }

            receiver.inventoryLink.AddItem(theItem, qty);
            targetInventoryManager.inventoryLink.RemoveItem(theItem, qty);
        }
        modified = true;
    }
	
    void Update() 
    {
        if (modified)
            UpdateDisplay();
    }
    public void selectCancelButton(BaseEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
    }
    #endregion
}