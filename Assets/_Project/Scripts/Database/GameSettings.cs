using UnityEngine;
using Events;
using Variables;
using UnityEngine.Audio;
using System.Runtime.Remoting.Messaging;

[CreateAssetMenu(menuName = "Data/Game Settings")]
public class GameSettings : ScriptableObject
{
    #region Structures
    public enum KeyTypes { Sound, Graphics, Controls, GUI, Gameplay };
    [System.Serializable]
    public struct ConfigKey
    {
        public string key;
        public float value, defaultValue;
        public KeyTypes keyType;
    }
    #endregion

    const string ckVersion = "configVersion";
    const string ckAutoReload = "autoReload";
    const string ckDrawNumbers = "drawNumbers";
    const string ckDrawHPBars = "drawHPBars";
    const string ckGameDifficulty = "gameDifficulty";

    [Header("Settings")]
    public bool showFloatingText;
    public bool showDamageNumbers;
    public bool autoReload = true;
    public bool drawNumbers = false;
    public bool drawHPBars = false;
    public float gameDifficulty = 1;
    public AudioMixer masterMixer;
    public ConfigKey[] configOptions;

    [Header("Helpers")]
    [SerializeField] GameEvent floatingTextRequestEvent;
    [SerializeField] FloatingTextRequest floatingTextRequestData;

    [Header("Serialization")]
    [SerializeField] float version = 2;

    bool mismatchedVersion = false;

    public float GetDifficultyModifier(bool useDifficultySetting)
    {
        if (useDifficultySetting)
            switch (Mathf.RoundToInt(gameDifficulty))
            {
                case 0: return 0.5f;
                case 2: return 1.5f;
                case 3: return 2f;
                default: return 1;
            }
        else
            return 1;
    }
    void SendFloatingTextRequest()
    {
        if(floatingTextRequestEvent)
            floatingTextRequestEvent.Raise();
    }
    public void FloatSomeText(string text, Color color, Transform target)
    {
        if (!showFloatingText) return;
        if (!floatingTextRequestData)
        {
            Debug.LogWarning(
                "Game Settings does not have a floatingTextRequestData object to use for sending requests for floating text.\n" +
                "Floating Text will not be visible.");
            return;
        }

        floatingTextRequestData.Set(
            text,
            color,
            target);

        SendFloatingTextRequest();
    }
    public void FloatSomeText(string text, Color color, Vector3 fixedPos)
    {
        if (floatingTextRequestData)
            floatingTextRequestData.Set(
                text,
                color,
                fixedPos);

        SendFloatingTextRequest();
    }

    //PlayerPrefs Methods
    float LoadKey(string key, float defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetFloat(key);
        else
            return defaultValue;
    }
    void SaveKey(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public void Load(bool setOption = true)
    {
        //Version
        float loadedVersion = LoadKey(ckVersion, 0);
        if (loadedVersion != version)
        {
            mismatchedVersion = true;
            Debug.LogWarning("Config version mismatch, loaded version = " + loadedVersion + ", current version = " + version);
        }

        //Game Difficulty
        gameDifficulty = LoadKey(ckGameDifficulty, 1);
        //Auto Reload
        autoReload = (LoadKey(ckAutoReload, 1) == 1 ? true : false);
        //Floating Numbers
        drawNumbers = (LoadKey(ckDrawNumbers, 1) == 1 ? true : false);
        //Floating HP Bars
        drawHPBars = (LoadKey(ckDrawHPBars, 1) == 1 ? true : false);

        //Generic options (Sound, etc)
        for (int i = 0; i < configOptions.Length; i++)
        {
            configOptions[i].value = LoadKey(configOptions[i].key, configOptions[i].defaultValue);

            if (setOption)
                switch (configOptions[i].keyType)
                {
                    case KeyTypes.Sound:
                        if (masterMixer)
                            Common.setMixerVolume(masterMixer, configOptions[i].key, configOptions[i].value);
                        else
                            Debug.Log("Audio Mixer is unassigned!");
                        break;
                    default:
                        Debug.Log("Unhandled Config Key Type: " + configOptions[i].keyType);
                        break;
                }
        }
    }
    public void Save()
    {
        //Version
        SaveKey(ckVersion, version);
        //Game Difficulty
        SaveKey(ckGameDifficulty, gameDifficulty);
        //Auto Reload
        SaveKey(ckAutoReload, autoReload ? 1 : 0);
        //Floating Numbers
        SaveKey(ckDrawNumbers, drawNumbers ? 1 : 0);
        //Floating Numbers
        SaveKey(ckDrawHPBars, drawHPBars ? 1 : 0);

        //Generic options (Sound, etc)
        for (int i = 0; i < configOptions.Length; i++)
            SaveKey(configOptions[i].key, configOptions[i].value);
    }

    public float GetKey(string key, float defaultValue)
    {
        foreach (ConfigKey ck in configOptions)
            if (ck.key == key)
                return ck.value;

        return defaultValue;
    }
    public void SetKey(string key, float value)
    {
        for (int i = 0; i < configOptions.Length; i++)
            if (configOptions[i].key == key)
            {
                configOptions[i].value = value;
                break;
            }
    }

    public void Startup()
    {
        Load();
        if (mismatchedVersion) Save();
    }
}