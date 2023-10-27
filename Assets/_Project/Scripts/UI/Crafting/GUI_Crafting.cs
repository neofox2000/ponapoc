using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RPGData;

public class GUI_Crafting : MonoBehaviour
{
    const float baseReturnRate = 0.1f;

    public class ShoppingListItem
    {
        //public int itemID;
        public ItemTemplate itemTemplate;
        public int quantity;
        public BaseItem itemLink;

        public ShoppingListItem(ItemTemplate itemTemplate, int quantity)
        {
            //this.itemID = itemID;
            this.quantity = quantity;
            this.itemTemplate = itemTemplate;
            itemLink = null;
        }

        public bool hasEnough(float skillValue)
        {
            return 
            (
                (itemLink != null) && 
                (itemLink.quantity >= GetCraftResourceRate(quantity, skillValue))
            );
        }
    }

    public RectTransform craftingPanel, disassemblyPanel;
    public GUI_InventoryManager inventory;
    public GUI_CraftingRecipeList IREManager;
    public Button disassembleActionButton, craftingActionButton;
    public Image disassembleItemIcon, craftingIcon;
    public Text 
        disassembleText, craftingText, 
        disassembleProgressText, craftingProgressText;
    public Slider disassembleProgressSlider, craftingProgressSlider;
    List<ShoppingListItem> craftingComponents = new List<ShoppingListItem>();

    BaseItem _selectedItem = null;
    public BaseItem selectedItem
    {
        get { return _selectedItem; }
        set
        {
            _selectedItem = value;
            SetDisassablyItem();
        }
    }
    
    ItemRecipeEx _selectedRecipe = null;
    public ItemRecipeEx selectedRecipe
    {
        get { return _selectedRecipe; }
        set
        {
            _selectedRecipe = value;
            setCraftingItem();
        }
    }
    
    public Inventory playerInventory
    {
        get { return inventory.inventoryLink; }
        //set { inventory.inventoryLink = value; }
    }
    public ItemRecipeProgressList playerIREManager
    {
        get { return IREManager.itemRecipeManagerLink; }
        //set { IREManager.itemRecipeManagerLink = value; }
    }

    //Cached calculated stats
    //float itemCraftQuantityModifier = 1f;
    float itemCraftMaterialCostModifier = 1f;
    float itemCraftDeconstructModifier = 1f;

    UI_Pagecontrol pageControl;
    PlayerController PC = null;

    //Standard Methods
    void Start()
    {
        disassembleText.text = string.Empty;
        pageControl = GetComponent<UI_Pagecontrol>();
        pageControl.OnActiveTabChanged += OnActiveTabChanged;

        IREManager.OnRowSubmit += OnRecipeClicked;

        OnActiveTabChanged(0);
    }
    /// <summary>
    /// Call setup with a null player parameter to disconnect it
    /// nb: This MUST be called before destroying connected controllers or there will be memory leaks!
    /// </summary>
    /// <param name="player"></param>
    public void Setup(PlayerController player)
    {
        PC = player;
        UpdateDisplay();
    }
    void UpdateDisplay()
    {
        if (PC != null)
        {
            if (disassemblyPanel.gameObject.activeSelf)
            {
                //Set the refresh flag
                inventory.refresh = true;

                //Reselect last
                selectedItem = selectedItem;

                //Set UI selection to first available
                disassemblyPanel.GetComponentInChildren<Selectable>().Select();
            }

            if (craftingPanel.gameObject.activeSelf)
            {
                //Set the refresh flag
                IREManager.refresh = true;

                //Reselect last
                selectedRecipe = selectedRecipe;

                //Set UI selection to first available
                craftingPanel.GetComponentInChildren<Selectable>().Select();
            }
        }
    }
    float GetItemTagModifiers(ItemTemplate itemTemplate)
    {
        return 1f;

        /*
        switch (itemType)
        {
            case ItemTypes.Consumable:
                return playerAlchemySkill.valueModded;
            default:
                return playerMechanicSkill.valueModded;
        }*/
    }
    int getQuantity(BaseItem item)
    {
        if (item != null)
            return item.quantity;
        else
            return 0;
    }
    void OnActiveTabChanged(int tabIndex)
    {
        selectedItem = null;
        selectedRecipe = null;

        UpdateDisplay();
    }


    //Disassembly Methods
    void SetDisassablyItem()
    {
        if (selectedItem != null)
        {
            Common.SetupIcon(disassembleItemIcon, selectedItem.template.icon, selectedItem.template.iconColor);
            disassembleItemIcon.enabled = true;
            disassembleActionButton.interactable = true;
        }
        else
        {
            disassembleItemIcon.sprite = null;
            disassembleItemIcon.enabled = false;
            disassembleActionButton.interactable = false;
        }

        updateDisassemblyProgressDisplay();
    }
    void updateDisassemblyProgressDisplay()
    {
        bool notAvailable = true;

        if (selectedItem != null)
        {
            ItemRecipeEx ire = IREManager.itemRecipeManagerLink.getItem(selectedItem.template);
            if (ire != null)
            {
                notAvailable = false;
                disassembleProgressText.text = "Level " + ire.level;
                disassembleProgressSlider.value = (float)ire.progress / (float)ire.progressRequired;
            }

        }

        if (notAvailable)
        {
            disassembleProgressText.text = "Level 0";
            disassembleProgressSlider.value = 0;
        }

    }
    public void OnItemClicked(GUI_InventoryItemRow itemRow, SubmitState state)
    {
        selectedItem = (BaseItem)itemRow.sourceLink;
        //inventory.drillDownSelection.OnDrillUp();
        disassembleActionButton.Select();
    }
    public void OnDisassableActionClick()
    {
        //if (disassemblyComponents.Count > 0)
        if (selectedItem.template.recipeReqs.Length > 0)
        {
            //string returnMessage = "";
            int returnQuantity;
            BaseItem returnedItem;
            string msg = string.Empty;

            //Remove item if equipped
            if ((selectedItem.equipped) && (selectedItem.quantity == 1))
                PC.myActor.ChangeEquipment(selectedItem, false);

            //Remove item from player inventory
            playerInventory.RemoveItem(selectedItem, 1);

            foreach (RecipeRequirement comp in selectedItem.template.recipeReqs)
            {
                returnQuantity = GetDisassembleResourceRate(
                    comp.quantity, 
                    itemCraftDeconstructModifier, 
                    false);

                if (returnQuantity > 0)
                {
                    returnedItem = playerInventory.AddItem(new BaseItem(comp.item, returnQuantity, 1, 0));

                    if (msg != string.Empty)
                        msg += "\n";

                    msg += string.Concat("Gained: ", returnedItem.template.name, " x", returnQuantity);
                    //returnMessage += "\n" + returnedItem.itemname + " x" + returnQuantity;
                }
            }

            if (msg == string.Empty)
                msg = "Lost all materials!";

            Alerter.ShowMessage(msg);

            //if (returnMessage != "")
            //GUI_MessagePanel.showMessage("Got" + returnMessage);

            IREManager.itemRecipeManagerLink.updateItemProgress(selectedItem.template);

            //Refresh Display
            if (!playerInventory.items.Contains(selectedItem))
                selectedItem = null;
            else
                selectedItem = selectedItem;
                //updateDisassemblyProgressDisplay();
        }
    }

    //Crafting Methods
    void linkShoppingListToInventory()
    {
        foreach (ShoppingListItem shoppingItem in craftingComponents)
            shoppingItem.itemLink = playerInventory.FindItem(shoppingItem.itemTemplate);
    }
    void cacheCraftingComponents()
    {
        craftingComponents.Clear();

        //Make shopping list
        //for (int i = 0; i < ItemRecipes.Instance.Rows.Count; i++)
        for (int i = 0; i < selectedRecipe.item.recipeReqs.Length; i++)
            craftingComponents.Add(new ShoppingListItem(
                selectedRecipe.item.recipeReqs[i].item,
                GetCraftResourceRate(
                    selectedRecipe.item.recipeReqs[i].quantity,
                    itemCraftMaterialCostModifier)));

        //Find relevant items in player inventory and cache them
        linkShoppingListToInventory();

        //Display info on UI
        if (craftingComponents.Count > 0)
        {
            craftingText.text = "You will need:";
            foreach (ShoppingListItem comp in craftingComponents)
            {
                craftingText.text += string.Concat("\n  ",
                    comp.itemTemplate.name,
                    " x", GetCraftResourceRate(comp.quantity, itemCraftMaterialCostModifier),
                    " (", getQuantity(comp.itemLink), ")");
            }
        }
        else
            craftingText.text = "This item cannot be broken down any further";

    }
    void clearCraftingComponents()
    {
        craftingComponents.Clear();
        craftingText.text = "Choose an item";
    }
    void setCraftingItem()
    {
        if (selectedRecipe != null)
        {
            Common.SetupIcon(craftingIcon, selectedRecipe.item.icon, selectedRecipe.item.iconColor);
            craftingIcon.enabled = true;
            craftingActionButton.interactable = true;
            cacheCraftingComponents();
        }
        else
        {
            craftingIcon.sprite = null;
            craftingIcon.enabled = false;
            craftingActionButton.interactable = false;
            clearCraftingComponents();
        }

        updateCraftProgressDisplay();
    }
    void updateCraftProgressDisplay()
    {
        if (selectedRecipe != null)
        {
            craftingProgressText.text = "Level " + selectedRecipe.level;
            craftingProgressSlider.value = (float)selectedRecipe.progress / (float)selectedRecipe.progressRequired;
        }
        else
        {
            craftingProgressText.text = "Level 0";
            craftingProgressSlider.value = 0;
        }
    }
    public void OnRecipeClicked(GUI_CraftingItemRow recipeRow)
    {
        selectedRecipe = (ItemRecipeEx)recipeRow.sourceLink;
        //IREManager.drillDownSelection.OnDrillUp();
        craftingActionButton.Select();
    }
    public void OnCraftActionClick()
    {
        if (craftingComponents.Count < 1) return;
        if (selectedRecipe.level < 1)
        {
            Alerter.ShowMessage("At least Level 1 is required to craft this!");
            return;
        }

        //Check to see if player has all required items
        bool hasEnough = true;
        linkShoppingListToInventory();
        foreach (ShoppingListItem shoppingItem in craftingComponents)
            if (!shoppingItem.hasEnough(itemCraftMaterialCostModifier))
            {
                hasEnough = false;
                break;
            }

        //Add the newly crafted item and remove craft materials used
        if (hasEnough)
        {
            BaseItem theItem;

            //Remove crafting materials
            foreach (ShoppingListItem shoppingItem in craftingComponents)
                playerInventory.RemoveItem(
                    shoppingItem.itemLink.template,
                    GetCraftResourceRate(shoppingItem.quantity, itemCraftMaterialCostModifier));

            //Create newly crafted item and set values based on crafting level
            theItem = new BaseItem(selectedRecipe.item, 1, 1, 0);
            SetCraftQQ(theItem, selectedRecipe.level);

            //Notify player
            Alerter.ShowMessage(string.Concat("(Q", theItem.quality, ") ", selectedRecipe.item.name, " x", theItem.quantity));

            //Add newly crafted item
            playerInventory.AddItem(theItem);

            //Update the recipe progress
            IREManager.itemRecipeManagerLink.updateItemProgress(selectedRecipe.item);

            //Refresh display
            selectedRecipe = selectedRecipe;
        }
        else
            Alerter.ShowMessage("Not enough materials!");

    }


    //Static Methods
    public static int GetDisassembleResourceRate(int resourceAmount, float skillValue, bool useMinimum)
    {
        float returnRate = Mathf.Clamp((baseReturnRate + ((skillValue / 111))), 0, 1);
        int returnQuantity = Mathf.RoundToInt(resourceAmount * returnRate);

        //Debug.Log("Base Return: " + resourceAmount + " / New Return: " + returnQuantity + " --- Return Rate: " + returnRate);
        if (returnQuantity < 1)
        {
            if (useMinimum)
                returnQuantity = 1;
            else
                returnQuantity = Random.Range(0, 2);
        }

        return returnQuantity;
    }
    public static int GetCraftResourceRate(int resourceAmount, float skillValue)
    {
        return Mathf.RoundToInt(resourceAmount * (2 - Mathf.Clamp((baseReturnRate + ((skillValue / 100))), 0, 1)));
        //return Mathf.RoundToInt((resourceAmount * (baseReturnRate + (baseReturnRate * (100 / skillValue)))));
    }
    public static void SetCraftQQ(BaseItem item, int craftingLevel)
    {
        //Don't make weapons with fully loaded clips
        if (item.template.itemType == ItemTypes.Weapon)
            item.chargesLeft = 0;

        //Set quality
        switch(item.template.itemType)
        {
            case ItemTypes.Ammo:
            case ItemTypes.Consumable:
                item.quantity = Mathf.RoundToInt(1 + (1 * Common.smoothFunctionFloat(craftingLevel, 0, 1.5f)));
                break;
            default:
                item.quality = craftingLevel;
                break;
        }
    }
}