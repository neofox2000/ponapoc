using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item Recipe Progress List")]
public class ItemRecipeProgressList : ScriptableObject, IListSource
{
    public List<ItemRecipeEx> list;
    IListRowSource[] IListSource.sourceItems
    {
        get
        {
            return list.ToArray();
        }
    }

    ItemRecipeEx cachedIRE = null;

    #region IListSource Methods
    void IListSource.Sort()
    {

    }
    void IListSource.SetSource(UI_ListManager list)
    {

    }
    void IListSource.UnsetSource(UI_ListManager list)
    {

    }

    #endregion

    #region Methods
    public ItemRecipeEx getItem(ItemTemplate itemTemplate)
    {
        if ((cachedIRE == null) || (cachedIRE.item != itemTemplate))
        {
            cachedIRE = null;
            foreach (ItemRecipeEx IRE in list)
                if (IRE.item == itemTemplate)
                    cachedIRE = IRE;
        }

        return cachedIRE;
    }
    public ItemRecipeEx updateItemProgress(ItemTemplate itemTemplate, int newProgress = 1)
    {
        ItemRecipeEx IRE = getItem(itemTemplate);
        if (IRE == null)
        {
            IRE = new ItemRecipeEx(itemTemplate);
            list.Add(IRE);
        }

        IRE.progress += newProgress;
        return IRE;
    }
    public SaveIRE[] getSaveIREs()
    {
        if (list.Count > 0)
        {
            SaveIRE[] ret = new SaveIRE[list.Count];
            for (int i = 0; i < list.Count; i++)
                ret[i] = new SaveIRE(list[i]);

            //Debug.Log(ret[0].ID + " = " + ret[0].progress);
            return ret;
        }
        else return new SaveIRE[0];
    }
    public void setSaveIREs(SaveIRE[] saveIREs)
    {
        list.Clear();
        if (saveIREs != null)
            foreach (SaveIRE IRE in saveIREs)
                list.Add(IRE.unpack());
    }
    public void onDeserialized()
    {

    }
    #endregion
}