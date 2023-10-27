using UnityEngine;

[System.Serializable]
public struct BaseStat
{
    #region Properties
    //Base value that will be saved/serialized
    public float valueBase;

    //Temporarily used when assigning points
    public float valueTemp { get; set; }
    
    //Temporarily used when buffs/debuffs affect this stat
    public float valueBonus { get; set; }
    
    //Combined value
    public float valueModded
    {
        get { return valueBase + valueBonus + valueTemp; }
    }
    
    //Combined value without buff bonuses
    public float valueBaseTemp
    {
        get { return valueBase + valueTemp; }
    }
    #endregion

    #region Methods
    public string GetDisplayName()
    {
        //For inherited classes
        return string.Empty;
    }
    public string GetDescription()
    {
        //For inherited classes
        return string.Empty;
    }
    public void ResetBonuses()
    {
        valueBonus = 0;
    }
    public void SaveTempValues()
    {
        valueBase += valueTemp;
        valueTemp = 0;
    }

    public BaseStat(float initialValue = 5f)
    {
        //Added to make sure that automatic initialization still works
        valueBase = initialValue;
        valueTemp = 0f;
        valueBonus = 0f;
    }
    #endregion
}