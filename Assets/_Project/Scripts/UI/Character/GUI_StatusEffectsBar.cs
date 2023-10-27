using UnityEngine;
using UnityEngine.UI;
using Variables;

public class GUI_StatusEffectsBar : MonoBehaviour
{
    public float refreshTime = 0.5f;
    public Vector2 textOffset = new Vector2(-20f, -4f);

    float refreshTimer = 0f;
    bool cacheComplete = false;

    GUI_StatusEffectBarIcon[] effectIcons;
    Image img;

    void OnEnable()
    {
        img = GetComponent<Image>();
        effectIcons = GetComponentsInChildren<GUI_StatusEffectBarIcon>();
        cacheComplete = true;
    }
    void Update()
    {
        if (!cacheComplete) return;

        if (refreshTimer > 0f)
            refreshTimer -= Time.deltaTime;
        else
        {
            refreshTimer = refreshTime;
            Refresh();
        }
    }

    void Refresh()
    {
        int activeBuffCount = 0;

        //Setup each buff found on the controller's character sheet
        foreach (StatusEffect effect in GameDatabase.lCharacterSheet.statusEffects)
        {
            if (activeBuffCount < effectIcons.Length)
            {
                //Cache buff
                effectIcons[activeBuffCount].statusEffectRef = effect;

                //Cache image
                Image image = effectIcons[activeBuffCount].sprite;

                //Turn on image if not already on
                if (!image.gameObject.activeSelf)
                    image.gameObject.SetActive(true);

                //Setup icon
                Common.SetupIcon(image, effect.icon, effect.iconColor);

                //Setup fill based on time remaining
                if (effect.duration > 0)
                    image.fillAmount = effect.GetRemainingTime() / effect.duration;
                else
                    image.fillAmount = 1;

                //Increment active buff counter
                activeBuffCount++;
            }
            else
                break;
        }

        //Hide remaining icons
        for (int i = activeBuffCount; i < effectIcons.Length; i++)
        {
            effectIcons[i].statusEffectRef = null;
            effectIcons[i].gameObject.SetActive(false);
        }

        //Show hide buff bar based on number of active buffs
        if (img) img.enabled = (activeBuffCount > 0);
    }

    public void OnHover(GUI_StatusEffectBarIcon icon)
    {
        if ((icon != null) && (icon.statusEffectRef != null))
        {
            //Show popup
            GUI_Common.instance.ShowDetailsPopup(
                icon.statusEffectRef.displayText,
                icon.GetComponent<RectTransform>(),
                TextAlignment.Left);
        }
    }
    public void OnHoverOut()
    {
        GUI_Common.instance.HideDetailsPopup();
    }
}