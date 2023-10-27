using UnityEngine;
using UnityEngine.UI;
using RPGData;

public class HUD_Bar : MonoBehaviour
{
    [Tooltip("Show 0 if value is negative?")]
    public bool noNegative = false;
    public bool showValueText = false;
    public bool showMaxValue = true;
    public bool expressAsPercentage = false;
    public int decimalPlaces = 0;
    public Text label, valueLabel;
    public Image fill;

    float currentValue = 0;
    string numberFormat;
    Attribute statCap = null;    //Uses the value from this stat for the max instead
    Attribute stat = null;

    void Awake()
    {
        if (decimalPlaces > 0)
        {
            numberFormat = "#,0.";
            for (int i = 0; i < decimalPlaces; i++)
                numberFormat += "0";
        }
        else
            numberFormat = Common.defaultIntFormat;        
    }
    void Update()
    {
        if (stat != null)
            SetValue(
                stat.valueCurrent,
                statCap == null ?
                    stat.valueModded :
                    statCap.valueModded);
    }

    /// <summary>
    /// Sets the header label (above bar)
    /// </summary>
    /// <param name="newtext">Text that will be shown.  (string.Empty hides label object)</param>
    public void SetLabel(string newtext)
    {
        if (label)
        {
            label.text = newtext;
            label.gameObject.SetActive(newtext != string.Empty);
        }
    }
    public void SetValue(float current, float max)
    {
        if (max > 0)
            currentValue = current / max;
        else
            currentValue = current;

        if (noNegative)
            currentValue = Mathf.Max(0, currentValue);

        if (valueLabel)
        {
            if (showValueText)
            {
                if (expressAsPercentage)
                    valueLabel.text = currentValue.ToString(numberFormat + "%");
                else
                {
                    if (showMaxValue)
                        valueLabel.text = current.ToString(numberFormat) + " / " + max.ToString(numberFormat);
                    else
                        valueLabel.text = current.ToString(numberFormat);
                }
            }

            valueLabel.gameObject.SetActive(showValueText);
        }

        //Set fill
        if (fill) fill.fillAmount = currentValue;
    }
    public void SetColor(Color color)
    {
        if (fill) fill.color = color;
    }
    public void SetAttribute(Attribute stat)
    {
        this.stat = stat;
        this.statCap = null;
    }
    public void SetAttribute(Attribute stat, Attribute maxStat)
    {
        this.stat = stat;
        this.statCap = maxStat;
    }
    public void UnsetAttribute()
    {
        this.stat = null;
        this.statCap = null;
    }
}