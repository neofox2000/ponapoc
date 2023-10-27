using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GUI_Common : MonoBehaviour
{
    public const string msgCloseAll = "OnCloseAll";

    public static GUI_Common instance;
    public enum MenuIDs
    {
        Hidden = -1,
        Main = 0,
        Options = 1,
        SaveLoad = 2,
        Credits = 3,
        Character = 4,
        Crafting = 5,
        Barter = 6,
        Transfer = 7,
        Help = 8
    }

    [Header("Widgets")]
    public Image background;
    public GUI_Savegames saveGames;
    [SerializeField] GUI_InventoryItemDetails itemDetailsPopup;
    [SerializeField] GUI_DetailsPopup detailsPopup;
    public GUI_InputBox guiInputBox;
    public GUI_MainMenuButtons mainMenuButtons;
    public GUI_CharacterSheet characterSheetGUI;
    public GUI_AbilitiesSheet abilitySheetGUI;
    public GUI_Crafting guiCrafting;
    public GUI_Barter guiBarter;
    public GUI_InventoryTransfer guiInventoryTransfer;
    public GUI_Help guiHelp;
    public Animator screenFader;

    [Header("Events")]
    public UnityEvent OnShow;
    public UnityEvent OnHide;

    [Header("The thing that should not be")]
    public StoryTemplate storyToPlayOnDeath;

    Animator animator;
    int akhMenuID, akhFadeOut, akhFadeDark;

    MenuIDs currentMenuID
    {
        get { return (MenuIDs)animator.GetInteger(akhMenuID); }
        set
        {
            animator.SetInteger(akhMenuID, (int)value);
            if (value == MenuIDs.Hidden)
            {
                HideSubPanels();

                //Fire event
                OnHide.Invoke();
            }
            else
            {
                OnShow.Invoke();
            }
        }
    }

    void Awake()
    {
        //Cache components
        animator = GetComponent<Animator>();

        //Cache animation hashes
        akhMenuID = Animator.StringToHash("MenuID");
        akhFadeOut = Animator.StringToHash("FadeOut");
        akhFadeDark = Animator.StringToHash("FadeDark");
    }
    void Start()
    {
        if (!instance)
        {
            instance = this;

            HideSubPanels();
            //detailsPopup.GetComponent<CanvasGroup>().alpha = 0;
            //itemDetailsPopup.GetComponent<CanvasGroup>().alpha = 0;
        }
        else
            if (instance != this)
            Destroy(gameObject);
    }

    public void ShowDetailsPopup(string text, RectTransform target, TextAlignment preferredAlignment)
    {
        if(IsAnythingShowing())
            detailsPopup.Setup(text, target, preferredAlignment);
    }
    public void HideDetailsPopup()
    {
        detailsPopup.ShowHide(false);
    }
    public void ShowItemDetails(BaseItem item, RectTransform target, TextAlignment preferredAlignment)
    {
        if (IsAnythingShowing())
            itemDetailsPopup.Setup(item, target, preferredAlignment);
    }
    public void ShowItemDetails(ItemTemplate item, RectTransform target)
    {
        if (IsAnythingShowing())
            itemDetailsPopup.Setup(item, target);
    }
    public void HideItemDetails()
    {
        itemDetailsPopup.ShowHide(false);
    }

    public void setBackground(bool enabled, Color color, Sprite sprite = null)
    {
        if (enabled)
        {
            background.sprite = sprite;
            background.color = color;
        }

        background.enabled = enabled;
    }
    public bool loadGame(int slotNo)
    {
        if (GameDatabase.core.LoadGame(slotNo))
        {
            currentMenuID = MenuIDs.Main;
            LoadingManager.SwitchScene(LoadingManager.SceneSelection.worldMap);
            GameDatabase.metaGame.currentState = Metagame.MetaGameStates.Idle;
            return true;
        }
        else Alerter.ShowMessage("No Savegame Available");

        return false;
    }
    public bool saveGame(int slotNo)
    {
        if (GameDatabase.metaGame.currentState == Metagame.MetaGameStates.Map)
        {
            GameDatabase.core.SaveGame(slotNo);
            Alerter.ShowMessage("Game Saved");
            return true;
        }
        else
            Alerter.ShowMessage("The game can only be saved from the World Map");

        return false;
    }
    public void onNew()
    {
        if (LoadingManager.currentScene == LoadingManager.SceneSelection.mission)
            GameManager.instance.ExitMission(Metagame.MetaGameStates.TitleMenu);

        LoadingManager.SwitchScene(LoadingManager.SceneSelection.charCreation);
    }
    public void onExit()
    {
        if (LoadingManager.currentScene == LoadingManager.SceneSelection.mission)
        {
            GameManager.instance.ExitMission(Metagame.MetaGameStates.TitleMenu);
            return;
        }

        if (LoadingManager.currentScene == LoadingManager.SceneSelection.worldMap)
        {
            LoadingManager.SwitchScene(LoadingManager.SceneSelection.title);
            GameDatabase.metaGame.currentState = Metagame.MetaGameStates.TitleMenu;
            return;
        }

        Application.Quit();
    }

    #region Events
    public void OnHelp(bool show)
    {
        if (show)
            currentMenuID = MenuIDs.Help;
        else
            currentMenuID = MenuIDs.Hidden;
    }
    public void OnMainMenu(bool show)
    {
        if (show)
            currentMenuID = MenuIDs.Main;
        else
            currentMenuID = MenuIDs.Hidden;
    }
    public void OnOptions(bool show)
    {
        if (show)
            currentMenuID = MenuIDs.Options;
        else
            currentMenuID = MenuIDs.Main;
    }
    public void OnSaveLoad(bool show)
    {
        //Don't allow saving while dead
        if (show && (GameDatabase.localPlayer.myActor.dead))
        {
            Alerter.ShowMessage("Cannot save while dead - try returning to the World Map first");
            return;
        }

        //Show/hide
        if (show)
        {
            currentMenuID = MenuIDs.SaveLoad;
            if(GameDatabase.metaGame.currentState == Metagame.MetaGameStates.TitleMenu)
                saveGames.mode = GUI_Savegames.Mode.LoadOnly;
            else
                saveGames.mode = GUI_Savegames.Mode.SaveOnly;
        }
        else
            currentMenuID = MenuIDs.Main;
    }
    public void OnCredits(bool show)
    {
        if (show)
        {
            currentMenuID = MenuIDs.Credits;
            OnFadeDark(true, 1f);
        }
        else
        {
            currentMenuID = MenuIDs.Main;
            OnFadeDark(false, 1f);
        }
    }
    public void OnCharacter(bool show)
    {
        if (show)
        {
            //Prevent opening if dead
            if (!GameDatabase.localPlayer.myActor.dead)
            {
                currentMenuID = MenuIDs.Character;
                characterSheetGUI.Show(GUI_CharacterSheet.CharacterSheetModes.Normal);
            }
        }
        else
        {
            if (characterSheetGUI.mode != GUI_CharacterSheet.CharacterSheetModes.Creation)
            {
                characterSheetGUI.Hide();
                currentMenuID = MenuIDs.Hidden;
            }
        }
    }
    public void OnCharacterCreation(bool show)
    {
        if (show)
        {
            currentMenuID = MenuIDs.Character;
            characterSheetGUI.Show(GUI_CharacterSheet.CharacterSheetModes.Creation);
        }
        else
        {
            characterSheetGUI.Hide();
            currentMenuID = MenuIDs.Hidden;
        }
    }
    public void OnCrafting(bool show)
    {
        if (show)
        {
            currentMenuID = MenuIDs.Crafting;
        }
        else
        {
            currentMenuID = MenuIDs.Hidden;
            guiCrafting.Setup(null);
        }
    }
    public void OnTransfer(bool show, PlayerController player, Inventory target, string targetName)
    {
        if (show)
        {
            currentMenuID = MenuIDs.Transfer;
            guiInventoryTransfer.Connect(player, target, targetName);
        }
        else
        {
            itemDetailsPopup.ShowHide(false);
            currentMenuID = MenuIDs.Hidden;
            guiInventoryTransfer.Connect(null, null, string.Empty);
        }
    }
    public void OnToggleButtons()
    {
        OnMainMenu(!mainMenuButtons.isShowing());
    }
    public void OnToggleCharacter()
    {
        OnCharacter(currentMenuID != MenuIDs.Character);
    }
    public void OnToggleHelp()
    {
        OnHelp(currentMenuID != MenuIDs.Help);
    }
    public void OnMenuButtonPressed()
    {
        //If question box is showing, default to cancel
        if (GUI_YesNoBox.instance.isShowing())
        {
            GUI_YesNoBox.instance.OnCancel();
            return;
        }

        //If sub menus showing, back out to main
        switch (currentMenuID)
        {
            case MenuIDs.Main:
                OnMainMenu(false);
                break;
            case MenuIDs.Options:
                OnOptions(false);
                break;
            case MenuIDs.SaveLoad:
                OnSaveLoad(false);
                break;
            case MenuIDs.Credits:
                OnCredits(false);
                break;
            case MenuIDs.Character:
                OnCharacter(false);
                break;
            case MenuIDs.Crafting:
                OnCrafting(false);
                break;
            case MenuIDs.Barter:
                guiBarter.CancelTrade();
                break;
            case MenuIDs.Transfer:
                guiInventoryTransfer.TransferEnded();
                break;
            default:
                //If nothing showing, open main menu
                OnMainMenu(true);
                break;
        }
    }
    public void OnCloseAll()
    {
        detailsPopup.ShowHide(false);
        itemDetailsPopup.ShowHide(false);
        characterSheetGUI.Hide();
        currentMenuID = MenuIDs.Hidden;
    }
    public void OnDead()
    {
        OnCloseAll();
        GUIManager.instance.PlayStory(storyToPlayOnDeath);
    }
    public void OnFadeOut(bool show, float fadeTime)
    {
        screenFader.speed = fadeTime;
        screenFader.SetBool(akhFadeOut, show);
    }
    public void OnFadeDark(bool show, float fadeTime)
    {
        screenFader.speed = fadeTime;
        screenFader.SetBool(akhFadeDark, show);
    }

    public void StartBarter()
    {
        currentMenuID = MenuIDs.Barter;
        guiBarter.Activate();
    }
    public void EndBarter()
    {
        guiBarter.DeActivate();
        itemDetailsPopup.ShowHide(false);
        currentMenuID = MenuIDs.Hidden;
    }
    #endregion

    public bool IsAnythingShowing()
    {
        return (currentMenuID != MenuIDs.Hidden);
    }
    public bool IsBartering()
    {
        return currentMenuID == MenuIDs.Barter;
    }
    public bool IsNonCharacterShowing()
    {
        return 
            (currentMenuID != MenuIDs.Hidden) &&
            //(currentMenuID != MenuIDs.Abilities) &&
            (currentMenuID != MenuIDs.Character);
    }
    public void HideSubPanels()
    {
        itemDetailsPopup.ShowHide(false);
        detailsPopup.ShowHide(false);
    }
}