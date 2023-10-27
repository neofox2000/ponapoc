using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_Pagecontrol : MonoBehaviour 
{
    [Serializable]
    public struct Page
    {
        public CanvasGroup panel;
        public Button tab;
        public bool invisible;
    }

    public Page[] pages;

    public Action<int> OnActiveTabChanged;

    int currentPageIndex = 0;
    int visibleTabCount
    {
        get
        {
            int ret = 0;
            if(pages != null)
            {
                foreach (Page page in pages)
                    if (!page.invisible)
                        ret++;
            }

            return ret;
        }
    }

    private void Awake()
    {
        //Cache current active page
        for(int i = 0; i < pages.Length; i++)
            if(pages[i].panel.gameObject.activeSelf)
            {
                currentPageIndex = i;
                break;
            }
    }
    public void OnTabClick(int pID)
    {
        HideAllPages();
        ShowPage(pID);

        if (OnActiveTabChanged != null)
            OnActiveTabChanged(pID);
    }
    public void NextTab()
    {
        int desiredPage = currentPageIndex + 1;
        if (desiredPage >= pages.Length)
            desiredPage = 0;

        //Is this page visible?
        if (pages[desiredPage].invisible)
        {
            //Skip invisible page
            currentPageIndex = desiredPage;
            NextTab();
        }
        else
            //Show visible page
            OnTabClick(desiredPage);
    }
    public void PrevTab()
    {
        int desiredPage = currentPageIndex - 1;
        if (desiredPage < 0)
            desiredPage = pages.Length - 1;

        //Is this page visible?
        if (pages[desiredPage].invisible)
        {
            //Skip invisible page
            currentPageIndex = desiredPage;
            PrevTab();
        }
        else
            //Show visible page
            OnTabClick(desiredPage);
    }

    void ShowPage(int pageID)
    {
        currentPageIndex = pageID;

        //Make tab non-interactable (should visually change tab to show it is the one showing)
        pages[pageID].tab.interactable = false;

        //Enable panel
        pages[pageID].panel.gameObject.SetActive(true);
    }
    void HideAllPages()
    {
        foreach (Page P in pages)
        {
            //Make tab interactable again
            P.tab.interactable = true;

            //Disable panel
            P.panel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Changes tab's usuability
    /// Hidden tabs cannot be activated until shown again using this function
    /// </summary>
    /// <param name="tabIndex">Duh</param>
    /// <param name="show">true = shown/useable, false = hidden/non-useable</param>
    public void ShowHideTab(int tabIndex, bool show)
    {
        //Is this a hide operation?
        if (!show)
        {
            //Prevent hiding the only visible tab
            if (visibleTabCount <= 1)
            {
                Debug.LogError("Cannot hide the only visible tab!");
                return;
            }

            //If trying to hide current tab, move to next first
            if (currentPageIndex == tabIndex)
                NextTab();
        }

        //Show or Hide tab
        pages[tabIndex].tab.gameObject.SetActive(show);
        //Set flag
        pages[tabIndex].invisible = !show;
    }
}
