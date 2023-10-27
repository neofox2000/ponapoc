using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ListManager : MonoBehaviour
{
    public GameObject itemRowPrefab;
    public RectTransform table;
    public ScrollRect scrollRect;

    //bool debugging = false;
    int lastCapacity = 0;
    protected List<UI_ListItem> rows;

    UI_ListItem _highlightedItem = null;
    public UI_ListItem highlightedItem
    {
        get { return _highlightedItem; }
    }

    IListSource _sourceLink = null;
    public IListSource sourceLink
    {
        get { return _sourceLink; }
        set 
        {
            //Redundancy check
            if (_sourceLink != value)
            {
                //Remove events
                if (_sourceLink != null)
                    _sourceLink.UnsetSource(this);

                //Add events
                if (value != null)
                    value.SetSource(this);

                //Update property
                _sourceLink = value;
            }
        }
    }

    public bool refresh { get; set; }

    //Event Handlers
    public virtual void OnListChanged()
    {
        refresh = true;
    }
    public virtual void OnListChanged(IListRowSource changedSource, bool removed)
    {
        refresh = true;
    }
    protected virtual void RowSubmit(UI_ListItem row, SubmitState stackMode)
    {
        GUI_Common.instance.HideItemDetails();
    }
    protected virtual void RowSelect(UI_ListItem row)
    {
        _highlightedItem = row;
    }
    protected virtual void RowCancel(UI_ListItem row)
    {
        //drillDownSelection.OnDrillUp();
    }
    protected virtual void RowScroll(PointerEventData eventData)
    {
        scrollRect.OnScroll(eventData);
    }

    //Monos
    protected virtual void OnEnable()
    {
        refresh = true;
    }
    protected virtual void Update()
    {
        if (refresh)
        {
            doRefresh();
            refresh = false;
        }
    }

    //Inits
    void makeNewItemRow()
    {
        //Instantiate prefab
        GameObject GO = Instantiate(itemRowPrefab) as GameObject;
        
        //Set Parent
        GO.transform.SetParent(table.gameObject.transform);

        //Fix Unity bugs (scale gets set to something other than 1 for no reason)
        RectTransform RT = GO.GetComponent<RectTransform>();
        RT.localScale = Vector3.one;

        //Get Component
        UI_ListItem newItemRow = GO.GetComponent<UI_ListItem>();

        //Subscribe to events
        newItemRow.OnSubmitted += RowSubmit;
        newItemRow.OnCanceled += RowCancel;
        newItemRow.OnSelected += RowSelect;
        newItemRow.OnScrolled += RowScroll;

        //Add to list
        rows.Add(newItemRow);
    }
    protected virtual void doRefresh()
    {
        if (sourceLink != null)
        {
            SetupDisplay();

            foreach (UI_ListItem itemRow in rows)
                itemRow.SetupDisplay();
        }
    }
    void SetCapacity(int newCapacity)
    {
        if (lastCapacity < newCapacity)
        {
            if (!itemRowPrefab)
                Debug.LogError("No Item Row prefab set!");
            else
            {
                for (int i = 0; i < newCapacity; i++)
                {
                    //Show previously hidden rows
                    if (i < rows.Count)
                        rows[i].gameObject.SetActive(true);
                    else
                    {
                        //Create new rows
                        makeNewItemRow();
                    }
                }
            }
        }
        else
        {
            //Hide extra rows
            for (int i = newCapacity; i < rows.Count; i++)
                rows[i].gameObject.SetActive(false);
        }

        //Setup row navigation


        lastCapacity = newCapacity;
    }
    protected void clear()
    {
        if (rows == null)
            rows = new List<UI_ListItem>();

        foreach (UI_ListItem itemRow in rows)
            itemRow.sourceLink = null;
    }
    protected virtual void SetupDisplay()
    {
        clear();
        if (sourceLink != null)
        {
            //Cache this to avoid multiple, slow ToArray calls
            IListRowSource[] sourceItems = sourceLink.sourceItems;

            //Cache this due to multiple uses
            int itemCount = sourceItems.Length;

            //Update the number of rows
            SetCapacity(itemCount);

            if (itemCount > 0)
            {
                //Sort list
                sourceLink.Sort();

                //Link UI rows to item data
                for (int i = 0; i < itemCount; i++)
                    rows[i].sourceLink = sourceItems[i];
            }
        }
    }
}
