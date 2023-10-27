using UnityEngine;
using UnityEngine.UI;
using RPGData;

public class GUI_InventoryItemDetails : GUI_HoverPanel 
{
    public Text 
        itemName, itemType, itemWeight, itemValue, 
        itemQuality, miscInfo;
    public GUI_ItemDetailStat[] labels;

    BaseItem itemShowing = null;

    void SetupSkillsDisplay(BaseItem item)
    {
        //Find applicable item skills and build a list
        string skillsUsed = string.Empty;

        //Skill Requirements
        /*
        if ((item.template.skills != null) && (item.template.skills.Length > 0))
        {
            skillsUsed = "\n\n" + "Skills: ";
            for (int i = 0; i < item.template.skills.Length; i++)
            {
                string skillName = item.template.skills[i]._skill.name;
                if (i == 0) skillsUsed += skillName; else skillsUsed += ", " + skillName;
            }
        }*/

        miscInfo.text += skillsUsed;
    }
    int SetupWeapon(BaseItem item)
    {
        ItemTemplate[] itemsList = GameDatabase.core.items;
        if (item.template.chargeUse > 0)
        {
            //Search for the ammo used
            string ammoUsed = string.Empty;
            for (int i = 0; i < itemsList.Length; i++)
                if ((itemsList[i].chargeType == item.template.chargeType) &&
                    (itemsList[i].itemType == ItemTypes.Ammo))
                {
                    ammoUsed = "\n" + "Uses " + itemsList[i].name;
                    break;
                }

            miscInfo.text += ammoUsed;
        }

        //Setup other labels
        labels[0].label.text = "Damage";
        labels[0].valueLabel.text = item.template.damageMatrix.ToString();

        labels[1].label.text = "Attack Speed";
        labels[1].valueLabel.text = item.template.useDelay.ToString(Common.defaultFloatFormat) + "s";

        if (item.template.chargeType > 0)
        {
            //Ranged Weapons
            labels[2].label.text = "Reload Time";
            labels[2].valueLabel.text = item.template.reloadDelay.ToString(Common.defaultFloatFormat) + "s";

            labels[3].label.text = "Shot Count";
            labels[3].valueLabel.text = item.template.shotCount.ToString();

            labels[4].label.text = "Shot Spread";
            labels[4].valueLabel.text = item.template.shotSpread.ToString(Common.defaultFloatFormat);

            labels[5].label.text = "Piercing Factor";
            labels[5].valueLabel.text = item.template.piercingFactor.ToString("#,0.00%");

            return 6;
        }
        else
        {
            //Melee Weapons
            labels[2].label.text = "Piercing Factor";
            labels[2].valueLabel.text = item.template.piercingFactor.ToString("#,0.00%");

            return 3;
        }
    }
    int SetupThrowable(BaseItem item)
    {
        //Setup other labels
        labels[0].label.text = "Damage";
        labels[0].valueLabel.text = item.template.damageMatrix.ToString();

        labels[1].label.text = "AoE Radius";
        labels[1].valueLabel.text = item.template.power != 0f ?
            item.template.power.ToString(Common.defaultFloatFormat) :
            "None";

        labels[2].label.text = "Distance";
        labels[2].valueLabel.text = item.template.range.ToString(Common.defaultFloatFormat);

        labels[3].label.text = "Cooldown";
        labels[3].valueLabel.text = item.template.useDelay.ToString(Common.defaultFloatFormat) + "s";

        return 4;
    }
    int SetupConsumeable(BaseItem item)
    {
        labels[0].label.text = "Duration";
        labels[0].valueLabel.text = item.power.ToString("#,0.00s");
        int usedLabels = 1;

        foreach (StatRule modifier in item.template.statusEffect.modifiers)
        {
            //Don't try to get labels beyond the number available
            if (usedLabels >= labels.Length) break;

            //Don't show hidden stat effects
            //if (!modifier.hidden)
            {
                //Display visible stat effects
                usedLabels++;
                labels[usedLabels - 1].label.text = modifier.affectedStat.ToString();
                labels[usedLabels - 1].valueLabel.text = modifier.percentage.ToString(Common.defaultFloatFormat);
            }
        }

        return usedLabels;
    }
    int SetupApparel(BaseItem item)
    {
        //Damage types
        labels[0].label.text = "Physical Protection";
        labels[0].valueLabel.text = item.template.damageMatrix.physical.ToString(Common.defaultIntPercentFormat);
        labels[1].label.text = "Chemical Protection";
        labels[1].valueLabel.text = item.template.damageMatrix.chemical.ToString(Common.defaultIntPercentFormat);
        labels[2].label.text = "Energy Protection";
        labels[2].valueLabel.text = item.template.damageMatrix.energy.ToString(Common.defaultIntPercentFormat);
        labels[3].label.text = "Biological Protection";
        labels[3].valueLabel.text = item.template.damageMatrix.biological.ToString(Common.defaultIntPercentFormat);

        return 4;
    }
    public void Setup(BaseItem item, RectTransform target, TextAlignment preferredAlignment, bool show = true)
    {
        //Set Item Name
        itemName.text = item.template.name;
        //Set Item Type
        itemType.text = item.template.itemType.ToString();
        //Set Item Weight
        itemWeight.text = item.weightTotal.ToString(Common.defaultFloatFormat);
        //Set Item Description
        miscInfo.text = item.template.description;
        //Set Item Value
        itemValue.text = item.template.value.ToString(Common.defaultFloatFormat);
        //Set Item Quality
        itemQuality.text = item.quality.ToString();

        //Skill Requirements
        SetupSkillsDisplay(item);

        //Set Type-specific stats
        int usedLabels = 0;
        switch (item.template.itemType)
        {
            case ItemTypes.Weapon:
                usedLabels = SetupWeapon(item);
                break;

            case ItemTypes.Throwable:
                usedLabels = SetupThrowable(item);
                break;

            case ItemTypes.Consumable:
                usedLabels = SetupConsumeable(item);
                break;

            case ItemTypes.Apparel:
                usedLabels = SetupApparel(item);
                break;
        }

        //Toggle field visibility as needed
        for (int i = 0; i < labels.Length; i++)
        {
            if (i < usedLabels)
                labels[i].gameObject.SetActive(true);
            else
                labels[i].gameObject.SetActive(false);
        }

        base.Setup(target, preferredAlignment, show);
    }
    public void Setup(ItemTemplate item, RectTransform target, bool show = true)
    {
        itemShowing = new BaseItem(item, 1, 1, 0);
        Setup(itemShowing, target, TextAlignment.Left, show);
    }
    public override void ShowHide(bool showIt = true)
    {
        base.ShowHide(showIt);
        itemShowing = null;
    }
}