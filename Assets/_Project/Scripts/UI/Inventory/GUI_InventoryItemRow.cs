using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RPGData;

public class GUI_InventoryItemRow : UI_ListItem
{
    [Header("Control Wiring")]
    public Image icon;
    public Text itemQuantity;
    public RectTransform equippedIndicator;

    //Event Triggers
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        //Hide basic details
        GUI_Common.instance.HideDetailsPopup();

        //Show item details
        GUI_Common.instance.ShowItemDetails((BaseItem) sourceLink, rTrans, TextAlignment.Left);
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        GUI_Common.instance.HideItemDetails();
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
    }

    //Main Methods
    /// <summary>
    /// Fills out all text and image fields with appropriate data from the item
    /// </summary>
    public override void SetupDisplay()
    {
        base.SetupDisplay();

        if (sourceLink != null)
        {
            BaseItem item = (BaseItem)sourceLink;

            //Icon
            Common.SetupIcon(icon, item.template.icon, item.template.iconColor);
            icon.enabled = true;

            //Toggle indicator if equipped
            if (equippedIndicator)
                equippedIndicator.gameObject.SetActive(item.equipped);

            //Show quantity if applicable
            if (itemQuantity)
            {
                if (item.template.isWeapon())
                {
                    if (item.template.chargeUse != 0)
                    {
                        itemQuantity.text = "(" + item.chargesLeft.ToString() + ")";
                        itemQuantity.gameObject.SetActive(true);
                    }
                    else
                        itemQuantity.gameObject.SetActive(false);
                }
                else
                {
                    itemQuantity.text = item.quantity.ToString();
                    itemQuantity.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if(icon) icon.sprite = null;
            if (equippedIndicator) equippedIndicator.gameObject.SetActive(false);
            if(itemQuantity) itemQuantity.text = string.Empty;
        }
    }

    /// <summary>
    /// Toggles interactable state based on matching itemtype
    /// </summary>
    /// <param name="itemType"></param>
    public bool ApplyFilter(ItemTypes itemType)
    {
        bool applicable = itemType == ((BaseItem)sourceLink).template.itemType;
        selectable.interactable = applicable;

        return applicable;
    }

    /// <summary>
    /// Resets the row state back to interactable
    /// </summary>
    public void RemoveFilter()
    {
        selectable.interactable = true;
    }
}