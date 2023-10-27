using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all learned abilities
/// </summary>
[Serializable]
public class AbilityConfig
{
    public List<BaseAbility> abilities;

    //Constructors
    public AbilityConfig()
    {
        abilities = new List<BaseAbility>();
    }
    public AbilityConfig(List<BaseAbility> abilities)
    {
        this.abilities = abilities;
    }
    public AbilityConfig(string serializedData)
    {
        abilities = new List<BaseAbility>();
        DeserializeFrom(serializedData);
    }

    //Serialization methods
    public string SerializeMe()
    {
        string[] sArray = new string[abilities.Count];
        for (int i = 0; i < abilities.Count; i++)
        {
            sArray[i] = string.Join(",", new string[]{
                abilities[i].template.saveID.ToString(),
                abilities[i].theoryLevel.ToString(),
                abilities[i].practiceLevel.ToString(),
                abilities[i].theoryXP.ToString(),
                abilities[i].practiceXP.ToString()
            });
        }

        return string.Join(";", sArray);
    }
    void DeserializeFrom(string data)
    {
        if (data != string.Empty)
        {
            int reading;
            BaseAbility ability;
            string[] sArray = data.Split(';');
            for (int i = 0; i < sArray.Length; i++)
            {
                string[] bits = sArray[i].Split(',');
                if (bits.Length == 5)
                {
                    //Try parsing all values and creating ability
                    ability = new BaseAbility(
                        int.TryParse(bits[0], out reading) ? reading : -1,
                        int.TryParse(bits[1], out reading) ? reading : 1,
                        int.TryParse(bits[2], out reading) ? reading : 1,
                        int.TryParse(bits[3], out reading) ? reading : 0,
                        int.TryParse(bits[4], out reading) ? reading : 0
                    );

                    //If ability wasn't rubbish, add it to the list
                    if (ability.template.saveID != -1)
                        abilities.Add(ability);
                    else
                        Debug.LogWarning("Invalid ability data found - could not parse ID to int!");
                }
                else
                    Debug.LogWarning("Invalid ability data found - wrong length!");
            }
        }
        else
            Debug.LogWarning("No AbilityConfig found in savegame (probably an old version)\nAbility config has been reset to default (none)");
    }
}