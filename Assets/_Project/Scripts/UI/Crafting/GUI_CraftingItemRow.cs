using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameDB;

public class GUI_CraftingItemRow : UI_ListItem 
{
    [Header("Control Wiring")]
    public Image icon;
    public Text details, quantity;

    //Event Triggers
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        GUI_Common.instance.ShowItemDetails(((ItemRecipeEx)sourceLink).item, rTrans);
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        GUI_Common.instance.HideItemDetails();
    }

    //Main Methods
    public override void SetupDisplay()
    {
        base.SetupDisplay();

        if (sourceLink != null)
        {
            ItemRecipeEx recipe = (ItemRecipeEx)sourceLink;
            Common.SetupIcon(icon, recipe.item.icon, recipe.item.iconColor);

            details.text = recipe.item.name;
            quantity.text = "1";
        }
        else
        {
            icon.sprite = null;
            details.text = string.Empty;
            quantity.text = string.Empty;
        }
    }
}
