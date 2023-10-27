using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_OptionsGameplay : MonoBehaviour 
{
    const string closeMessage = "OnMain";

    [SerializeField] GameSettings settings;
    public Dropdown dropdown_Difficulty;
    public Toggle autoReloadCheckbox;
    public Toggle floatingNumbersCheckbox;
    public Toggle floatingHealthBars;

    void close()
    {
        SendMessageUpwards(closeMessage);
    }

    public void OnShow()
    {
        settings.Load();
        dropdown_Difficulty.value = Mathf.RoundToInt(settings.gameDifficulty);
        autoReloadCheckbox.isOn = settings.autoReload;
        floatingNumbersCheckbox.isOn = settings.drawNumbers;
        floatingHealthBars.isOn = settings.drawHPBars;
    }
    public void OnSave()
    {
        settings.gameDifficulty = dropdown_Difficulty.value;
        settings.autoReload = autoReloadCheckbox.isOn;
        settings.drawNumbers = floatingNumbersCheckbox.isOn;
        settings.drawHPBars = floatingHealthBars.isOn;

        settings.Save();
        close();
    }
    public void OnCancel()
    {
        close();
    }
}