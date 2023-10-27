using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;

public class GUI_Savegames : MonoBehaviour, IScrollHandler
{
    public enum Mode { SaveOnly, LoadOnly }

    const string closeMessage = "OnSaveLoad";
    const string closeAllMessage = "OnCloseAll";
    const int rowCount = 20;

    public ScrollRect scrollRect;
    public GameObject rowPrefab;
    public Button cancelButton;

    GUI_SavegameItem[] rows = null;
    int selectedIndex = 0;
    bool _modified = true;
    public bool modified
    {
        get { return _modified; }
        set { _modified = value; }
    }
    public Mode mode { get; set; }

    void initRows()
    {
        GameObject GO;

        //Init statRows cache
        if ((rows == null) || (rows.Length != rowCount))
        {
            rows = new GUI_SavegameItem[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                GO = Instantiate(rowPrefab) as GameObject;
                GO.transform.SetParent(scrollRect.content.transform);

                //Fix Unity bugs (scale gets set to something other than 1 for no reason)
                RectTransform RT = GO.GetComponent<RectTransform>();
                RT.localScale = Vector3.one;

                rows[i] = GO.GetComponent<GUI_SavegameItem>();
                rows[i].init(i, 
                    OnSaveRowSelected,
                    OnSaveRowCanceled, 
                    OnSaveRowSubmitted, 
                    OnScroll);
            }
        }
    }
    public void refresh()
    {        
        //Setup stats if there are any available
        if (rowCount > 0)
        {
            GUI_SavegameItem saveRow;

            //Setup row objects
            initRows();

            //Populate row data
            for (int i = 0; i < rowCount; i++)
            {
                saveRow = rows[i];
                saveRow.setup(i);
            }
        }
    }

    //Mono internals
    void Update()
    {
        if (modified)
        {
            modified = false;
            refresh();
        }
    }
    void doSave(int slotNo)
    {
        modified = GUIManager.instance.guiCommon.saveGame(slotNo);
        SendMessageUpwards(closeAllMessage);
    }

    //Events
    public void OnScroll(PointerEventData eventData)
    {
        scrollRect.OnScroll(eventData);
    }
    public void OnSaveRowSelected(GUI_SavegameItem row)
    {
        //
    }
    public void OnSaveRowSubmitted(GUI_SavegameItem row)
    {
        selectedIndex = row.saveIndex;

        if (mode == Mode.SaveOnly)
            OnSaveGame(row);
        else
            OnLoadGame(row);
    }
    public void OnSaveRowCanceled()
    {
        OnCancelClick();
    }
    public void OnLoadGame(GUI_SavegameItem row)
    {
        if (rows[selectedIndex].hasData)
            GUI_YesNoBox.instance.Show(
                "Do you want to load this game?", true,
                () => { GUIManager.instance.guiCommon.loadGame(selectedIndex); },
                () => { row.selectable.Select(); },
                () => { row.selectable.Select(); });
        else
            Alerter.ShowMessage("Invalid Savegame!");
    }
    public void OnSaveGame(GUI_SavegameItem row)
    {
        if (rows[selectedIndex].hasData)
            GUI_YesNoBox.instance.Show(
                "Do you want to overwrite this slot?", true, 
                () => { doSave(selectedIndex); },
                () => { row.selectable.Select(); },
                () => { row.selectable.Select(); });
        else
            doSave(selectedIndex);
    }
    public void OnCancelClick()
    {
        SendMessageUpwards(closeMessage, false);
    }
}