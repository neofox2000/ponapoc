using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Variables;

public class HUD_Connector : MonoBehaviour 
{
    #region Structures
    //Constants
    const int floatingTextInstances = 25;
    const int barkInstances = 25;

    [System.Serializable]
    public struct StaticGUI
    {
        public RectTransform userInferface;
        [UnityEngine.Serialization.FormerlySerializedAs("guiBuffBar")]
        public GUI_StatusEffectsBar statusEffectsBar;
        public HUD_Bar hpBar, mpBar, xpBar, spBar, apBar;
        public HUD_ColorFlash colorFlash;
        public HUD_LevelPanel levelPanel;
        public HUD_Sneaking sneakingHud;
        public Image weightIndicator;
    }
    [System.Serializable]
    public struct GUISounds
    {
        public string statUpSound;
        public string skillUpSound;
    }
    #endregion

    //Inspector Properties
    [SerializeField] float lowLifeThreshold = 0.35f;
    [SerializeField] FloatingTextRequest floatingTextRequest;
    public GameObject 
        floatingTextPrefab, 
        floatingBarPrefab, 
        barkPrefab;
    public StaticGUI staticGUI;
    public GUISounds sounds;

    //Private Properties
    int nextFloatingTextID = 0;
    int akhStarving = Animator.StringToHash("Starving");
    Animator animator;
    HUD_FloatingText[] floatingTextObjects;
    HUD_Bark[] barkObjects;

    //Methods
    void OnEnable()
    {
        //Cache animator
        animator = GetComponent<Animator>();
        RegisterCharacterSheetEvents();
    }
    void OnDisable()
    {
        CleanupFloatingText();
        UnregisterCharacterSheetEvents();
    }
    void Start()
    {
        if (!staticGUI.userInferface.gameObject.activeInHierarchy)
            staticGUI.userInferface.gameObject.SetActive(true);

        floatingTextObjects = new HUD_FloatingText[floatingTextInstances];
        barkObjects = new HUD_Bark[barkInstances];

        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        while (!Camera.main)
            yield return null;

        GameObject GO;

        //Create a pool of floating text objects
        for (int i = 0; i < floatingTextObjects.Length; i++)
        {
            GO = Instantiate(floatingTextPrefab) as GameObject;
            GO.transform.SetParent(staticGUI.userInferface.gameObject.transform);
            GO.SetActive(false);
            floatingTextObjects[i] = GO.GetComponent<HUD_FloatingText>();
        }

        //Create a pool of bark objects
        for (int i = 0; i < barkObjects.Length; i++)
        {
            GO = Instantiate(barkPrefab) as GameObject;
            GO.transform.SetParent(staticGUI.userInferface.gameObject.transform);
            GO.SetActive(false);
            barkObjects[i] = GO.GetComponent<HUD_Bark>();
        }
    }
    void FixedUpdate()
    {
        animator.SetBool(akhStarving,
            GameDatabase.lCharacterSheet.SP.valueCurrent <= 15);

        staticGUI.weightIndicator.gameObject.SetActive(
            GameDatabase.lInventory.weight >= GameDatabase.lCharacterSheet.carryWeight);

        //HUD_Connector.instance.staticGUI.xpBar.setValue(characterSheet.XP, characterSheet.XPRequired);
    }

    public void Activate()
    {
        //Setup appropriate backgrounds and colors
        GUIManager.instance.guiCommon.setBackground(false, Color.black);
        GUIManager.instance.guiCommon.mainMenuButtons.Setup(
            GUI_MainMenuButtons.BackgroundMode.Normal, 
            GUI_MainMenuButtons.ButtonsMode.Both);
        GUIManager.instance.guiMission.gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        StartCoroutine(DelayedDeactivate());
    }
    IEnumerator DelayedDeactivate()
    {
        GUI_Common.instance.OnCloseAll();
        yield return new WaitForEndOfFrame();
        GUIManager.instance.guiMission.gameObject.SetActive(false);
    }

    HUD_FloatingText getFreeFloatingText()
    {
        HUD_FloatingText ret = floatingTextObjects[nextFloatingTextID];
        nextFloatingTextID++;
        if (nextFloatingTextID >= floatingTextObjects.Length)
            nextFloatingTextID = 0;

        return ret;
    }

    /// <summary>
    /// Instatiates Floating Hud Bar and assigns it to the target
    /// </summary>
    /// <param name="target">The object that the bar will follow</param>
    /// <returns>Returns the bar</returns>
    public HUD_Bar GetFloatingBar(Transform target, Vector3 offset)
    {
        //GameObject GO = NGUITools.AddChild(staticGUI.userInferface.gameObject, floatingBarPrefab);
        GameObject GO = Instantiate(floatingBarPrefab) as GameObject;
        GO.transform.SetParent(staticGUI.userInferface.transform);
        HUD_FloatingObject foo = GO.GetComponent<HUD_FloatingObject>();
        foo.fire(target);
        if(offset != Vector3.zero)
            foo.worldOffset = offset;
        return foo.GetComponent<HUD_Bar>();
    }
    /// <summary>
    /// Destroys floating bar
    /// </summary>
    /// <param name="bar"></param>
    void ReleaseFloatingBar(HUD_Bar bar)
    {
        if (bar)
            Destroy(bar.gameObject);
    }
    void MakeFloatingText(string text, Color textColor, Transform target)
    {
        HUD_FloatingText ft = getFreeFloatingText();
        if (ft != null)
            ft.fire(text, textColor, target);
        else
            Debug.LogWarning(target + " wants a floating text object, but no one came.");
    }
    void MakeFloatingText(string text, Color textColor, Vector3 fixedPosition)
    {
        getFreeFloatingText().fire(text, textColor, fixedPosition);
    }
    void CleanupFloatingText()
    {
        foreach (HUD_FloatingObject fo in floatingTextObjects)
            fo.hide();
    }
    void PainFlash()
    {
        staticGUI.colorFlash.doPainFlash();
    }
    void PainLowLife(bool lowLife)
    {
        staticGUI.colorFlash.setPainLowLife(lowLife);
    }
    void PlayerHPChanged(float newValue, float delta, float overflow)
    {
        //Pain flash
        if (delta < -0.5f) PainFlash();

        //Low life indicator
        PainLowLife((
            newValue /
            GameDatabase.lCharacterSheet.HP.valueModded) < lowLifeThreshold);
    }
    void RegisterCharacterSheetEvents()
    {
        GameDatabase.lCharacterSheet.HP.OnCurrentValueChanged += PlayerHPChanged;
        staticGUI.hpBar.SetAttribute(GameDatabase.lCharacterSheet.HP);
        staticGUI.mpBar.SetAttribute(GameDatabase.lCharacterSheet.MP);
        staticGUI.spBar.SetAttribute(GameDatabase.lCharacterSheet.SP);
        staticGUI.apBar.SetAttribute(GameDatabase.lCharacterSheet.AP,
                                     GameDatabase.lCharacterSheet.SP);
    }
    void UnregisterCharacterSheetEvents()
    {
        GameDatabase.lCharacterSheet.HP.OnCurrentValueChanged -= PlayerHPChanged;
        staticGUI.hpBar.UnsetAttribute();
        staticGUI.mpBar.UnsetAttribute();
        staticGUI.spBar.UnsetAttribute();
        staticGUI.apBar.UnsetAttribute();
    }

    #region Exclusively called from Animator or UI Event
    public void RegisterPlayerEvents()
    {
    }
    public void UnregisterPlayerEvents()
    {
    }
    public void OnReturnToMap()
    {
        GameManager.instance.ExitMission(Metagame.MetaGameStates.Abandon);
    }
    public void ProcessFloatingTextRequest()
    {
        if (floatingTextRequest.target)
        {
            MakeFloatingText(
                floatingTextRequest.text,
                floatingTextRequest.color,
                floatingTextRequest.target);

            //Clear target for next request
            floatingTextRequest.target = null;
        }
        else
            MakeFloatingText(
                floatingTextRequest.text,
                floatingTextRequest.color,
                floatingTextRequest.fixedPosition);
    }
    #endregion
}