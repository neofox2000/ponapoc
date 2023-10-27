using UnityEngine.UI;
using System;

public class GUI_CraftingRecipeList : UI_ListManager
{
    public Text nothingText;

    public ItemRecipeProgressList itemRecipeManagerLink
    {
        get { return (ItemRecipeProgressList)sourceLink; }
        set { sourceLink = value; }
    }

    public Action<GUI_CraftingItemRow> OnRowSubmit;

    protected override void RowSubmit(UI_ListItem itemRow, SubmitState state)
    {
        base.RowSubmit(itemRow, state);

        if (OnRowSubmit != null)
            OnRowSubmit((GUI_CraftingItemRow)itemRow);
    }
    protected override void SetupDisplay()
    {
        base.SetupDisplay();

        nothingText.gameObject.SetActive((sourceLink == null) || (sourceLink.sourceItems.Length == 0));
    }
}