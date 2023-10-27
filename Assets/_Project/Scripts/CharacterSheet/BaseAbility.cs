using System.Collections.Generic;

[System.Serializable]
public class BaseAbility
{
    //Template data
    public AbilityTemplate template;
    //How much this ability has been studied
    public BaseStat theoryLevel;
    //How much xp has been gained toward the next theory level
    public float theoryXP;
    //How much this ability has been used
    public BaseStat practiceLevel;
    //How much xp has been gained toward the next practice level
    public float practiceXP;

    public float cooldownTimer { get; protected set; }
    public float levelValueMeanBase { get { return (theoryLevel.valueBase + practiceLevel.valueBase) / 2; } }
    public float levelValueMeanModded { get { return (theoryLevel.valueBaseTemp + practiceLevel.valueBaseTemp) / 2; } }

    public BaseAbility(AbilityTemplate template, bool startup = true)
    {
        this.template = template;
        theoryLevel = new BaseStat(startup ? 1 : 0);
        practiceLevel = new BaseStat(startup ? 1 : 0);
        theoryXP = 0;
        practiceXP = 0;
        cooldownTimer = 0;
    }
    public BaseAbility(int saveID, float TL, float PL, float TXP, float PXP)
    {
        template = System.Array.Find(GameDatabase.core.abilities, x => x.saveID == saveID);
        theoryLevel = new BaseStat(TL);
        practiceLevel = new BaseStat(PL);
        theoryXP = TXP;
        practiceXP = PXP;
        cooldownTimer = 0;
    }

    public string getDisplayName()
    {
        return template.name;
    }
    public string getDescription()
    {
        return template.description;
    }
    public string getReadout()
    {
        return template.GetReadout();
    }
    void addTheoryExp(float exp)
    {
        //Add exp
        theoryXP += exp;

        //Check for level up if not at max level
        if ((theoryLevel.valueBase < template.maxLevel) && (theoryXP >= requiredTheoryXP()))
        {
            //Level up
            theoryXP -= requiredTheoryXP();
            theoryLevel.valueBase++;
        }
    }
    public float requiredTheoryXP()
    {
        //TODO Add a better equation for this
        return theoryLevel.valueBase * 1000;
    }
    void addPracticeExp(float exp)
    {
        if (exp < 0) return;

        //Add exp
        practiceXP += exp;

        //Check for level up if not at max level
        if ((practiceLevel.valueBase < template.maxLevel) && (practiceXP >= requiredPracticeXP()))
        {
            //Level up
            practiceXP -= requiredPracticeXP();
            practiceLevel.valueBase++;
        }
    }
    public float requiredPracticeXP()
    {
        //TODO Add a better equation for this
        return practiceLevel.valueBase * 1000;
    }

    //Cooldown Methods
    public bool isReady()
    {
        return cooldownTimer <= 0;
    }
    public void tick(float deltaTime)
    {
        if (cooldownTimer > 0)
            cooldownTimer -= deltaTime;
    }
    public void used(float learningModifier)
    {
        cooldownTimer = template.cooldown;
        addPracticeExp(
            //Learning modified exp
            (template.pxpPerUse * learningModifier) +
            //Theory Level modified exp (can be negative)
            (template.pxpPerUse * ((theoryLevel.valueBase - practiceLevel.valueBase) * 0.25f)));
    }
    public bool bookUsed(BaseItem book, float learningSkillValue)
    {
        if (theoryLevel.valueBase < template.maxLevel)
        {
            addTheoryExp(
                template.txpPerBook *
                learningSkillValue *
                book.power);
            return true;
        }

        return false;
    }

    //Other methods
    public void CopyFrom(BaseAbility source, bool copyRuntimeStuff)
    {
        template = source.template;
        theoryLevel = source.theoryLevel;
        theoryXP = source.theoryXP;
        practiceLevel = source.practiceLevel;
        practiceXP = source.practiceXP;

        if (copyRuntimeStuff)
        {
            cooldownTimer = source.cooldownTimer;
        }
    }
}

public class BaseAbilitySort : Comparer<BaseAbility>
{
    public override int Compare(BaseAbility x, BaseAbility y)
    {
        if (x.levelValueMeanBase != y.levelValueMeanBase)
            return -x.levelValueMeanBase.CompareTo(y.levelValueMeanBase);
        else
            return x.getDisplayName().CompareTo(y.getDisplayName());
    }
}