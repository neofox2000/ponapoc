using RPGData;
using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public IStatusEffectSource source { get; protected set; }
    public bool firedOnce = false;

    float timeAdded = 0;
    StatusEffectTemplate template { get { return source.GetTemplate(); } }

    //Accessors
    public StatRule[] modifiers
    {
        get { return template.modifiers; }
    }
    public Sprite icon
    {
        get { return template.icon; }
    }
    public Color32 iconColor
    {
        get { return template.iconColor; }
    }
    string readout()
    {
        string ret = string.Empty;
        if (modifiers != null)
        {
            foreach (StatRule mod in modifiers)
                //if (!mod.hidden)
                    ret = string.Concat(
                        ret,
                        mod.affectedStat, " ",
                        //Only display a power if it's high enough
                        mod.percentage > 0.5f ? mod.percentage.ToString("+0;-0;Zero") : "",
                        "\n");
        }
        return ret;
    }
    public float duration
    {
        get { return template.duration; }
    }
    public string displayText
    {
        get
        {
            string strDuration = string.Empty;
            if (duration > 0)
                strDuration = string.Concat(
                    "Duration ", GetRemainingTime().ToString("0"),
                    " / ",
                    duration.ToString());

            return string.Concat(
                template.displayName, "\n\n",   //Header + spacing
                strDuration, "\n",              //Duration
                readout());                     //List of effects        
        }
    }

    //Constructor
    public StatusEffect(IStatusEffectSource source)
    {
        this.source = source;
        ResetTimer();
    }

    //Upkeep
    public float GetRemainingTime()
    {
        //Permenant Status Effect
        if (duration == 0) return 999f;

        //Timed Status Effect
        float remainingTime = duration - (Time.time - timeAdded);
        if (remainingTime < 0)
            return 0;
        else
            return remainingTime;
    }
    public bool isExpired()
    {
        return GetRemainingTime() <= 0;
    }
    public void ResetTimer()
    {
        timeAdded = Time.time;
    }
}

public interface IStatusEffectSource
{
    StatusEffectTemplate GetTemplate();
}