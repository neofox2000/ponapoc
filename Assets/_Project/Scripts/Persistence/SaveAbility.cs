[System.Serializable]
public class SaveAbility
{
    public int ID;
    public float
        theoryValue,
        practiceValue,
        theoryXP,
        practiceXP;

    public SaveAbility()
    {
        ID = 0;
        theoryValue = 0;
        practiceValue = 0;
    }
    public SaveAbility(BaseAbility saveMe)
    {
        ID = saveMe.template.saveID;
        theoryValue = saveMe.theoryLevel.valueBase;
        practiceValue = saveMe.practiceLevel.valueBase;
        theoryXP = saveMe.theoryXP;
        practiceXP = saveMe.practiceXP;
    }
    public void unpack(BaseAbility unpackToMe)
    {
        unpackToMe.theoryLevel.valueBase = theoryValue;
        unpackToMe.practiceLevel.valueBase = practiceValue;
        unpackToMe.theoryXP = theoryXP;
        unpackToMe.practiceXP = practiceXP;
    }
}