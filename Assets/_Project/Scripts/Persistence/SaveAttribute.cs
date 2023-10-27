using RPGData;

[System.Serializable]
public class SaveAttribute
{
    public int ID;
    public float valueBase;
    public float valueCurrent;

    public SaveAttribute()
    {
        ID = 0;
        valueBase = 0f;
        valueCurrent = 0f;
    }
    public SaveAttribute(Attribute saveMe)
    {
        ID = saveMe.template.saveID;
        valueBase = saveMe.valueBase;
        valueCurrent = saveMe.valueCurrent;
    }
    public void Unpack(Attribute unpackToMe)
    {
        unpackToMe.valueBase = valueBase;
        unpackToMe.SetToFixedAmount(valueCurrent);
    }
}