using UnityEngine;
using Variables;

namespace RPGData
{
    [System.Serializable]
    public class Attribute
    {
        public enum ConsumptionMode { NoLimit, HardLimit, SoftLimit }

        public AttributeTemplate template;
        public float valueBase;
        public StringVariable unitOfMeasure;
        [Tooltip("Note: Only applies to `current value` checks\nNo Limit = Always\nHard Limit = Must have exactly/more\nSoftLimit = Must have more than 0")]
        public ConsumptionMode consumptionMode;
        public float regenRate;
        public float regenRateFixed;
        public bool ignoreBaseValue = false;
        [Tooltip("Current value and regen will not be used if this is disabled")]
        public bool enabled = true;

        /// <summary>
        /// Fires when current value changes
        /// Parameters: newValue, delta, overflow
        /// </summary>
        public System.Action<float, float, float> OnCurrentValueChanged;

        //Temporarily used when assigning points
        public float valueTemp { get; set; }

        //Temporarily used when buffs/debuffs affect this stat
        public float valueBonus { get; set; }

        //Total value
        public float valueModded
        {
            get { return valueBase + valueBonus + valueTemp; }
        }

        //Total value (minus buff bonuses)
        public float valueBaseTemp
        {
            get { return valueBase + valueTemp; }
        }

        //Value filler
        protected float _valueCurrent = 1f;
        public float valueCurrent
        {
            get { return _valueCurrent; }
            protected set
            {
                if (enabled)
                {
                    float overflow = 0;
                    float oldVal = _valueCurrent;
                    _valueCurrent = value;

                    //Enforce limitations
                    if ((!ignoreBaseValue) && (_valueCurrent > valueModded))
                    {
                        overflow = _valueCurrent - valueModded;
                        _valueCurrent = valueModded;
                    }

                    //Fire change notification
                    if ((OnCurrentValueChanged != null) && (oldVal != _valueCurrent))
                        OnCurrentValueChanged(
                            _valueCurrent,
                            _valueCurrent - oldVal,
                            overflow);
                }
            }
        }

        bool uomCached = false;
        string _uom = string.Empty; //Unit of Measure
        string uom
        {
            get
            {
                //Cache the value once since it should never change at runtime
                if (!uomCached)
                {
                    if (template && template.unitOfMeasure)
                        _uom = template.unitOfMeasure.Value;
                    else if (unitOfMeasure)
                        _uom = unitOfMeasure.Value;

                    uomCached = true;
                }

                return _uom;
            }
        }

        #region Methods
        public virtual void ResetBonuses()
        {
            valueBonus = 0;
            ResetRegenRates();
        }
        public virtual void SaveTempValues()
        {
            valueBase += valueTemp;
            valueTemp = 0;
        }

        public Attribute()
        {
            //Added to make sure that automatic initialization still works
        }
        public Attribute(AttributeTemplate template)
        {
            this.template = template;
        }
        public Attribute(float startValue)
        {
            valueBase = startValue;
        }
        public Attribute(ConsumptionMode consumptionMode = ConsumptionMode.NoLimit)
        {
            valueBase = 0f;
            this.consumptionMode = consumptionMode;
        }
        public Attribute(float startValue, ConsumptionMode consumptionMode = ConsumptionMode.NoLimit)
        {
            this.consumptionMode = consumptionMode;
        }
        public float DoRegen(float deltaTime = 0f)
        {
            float ret = 0;

            if (enabled && (deltaTime > 0) && ((regenRate != 0) || (regenRateFixed != 0)))
            {
                //Calculate amount to work with
                float amount =
                    (valueModded * regenRate * deltaTime) +
                    (regenRateFixed * deltaTime);

                //Regen
                if (amount > 0)
                {
                    valueCurrent += amount;
                }
                //Degen
                else
                {
                    //Flip the sign for easy use
                    amount *= -1;

                    if (HasEnough(amount))
                    {
                        ret = ConsumeLimited(amount);
                    }
                    else
                    {
                        ret = amount;
                    }
                }
            }

            return ret;
        }
        public void Disable(float value)
        {
            enabled = false;
            valueCurrent = value;
        }
        public void Disable(bool reduceToZero = true)
        {
            if (reduceToZero)
                Disable(0);
            else
                Disable(valueCurrent);
        }
        public void Enable()
        {
            enabled = true;
        }
        public void Enable(float percent, bool resetBonus = true)
        {
            Enable();
            //if (resetBonus) resetBonuses();
            valueCurrent = (valueModded / 100) * percent;
        }
        public string GetReadOut()
        {
            if (ignoreBaseValue)
                return string.Concat(
                    template.GetDisplayName(), ": ",
                    valueCurrent.ToString(Common.defaultIntFormat),
                    uom);
            else
                return string.Concat(
                    template.GetDisplayName(), ": ",
                    valueCurrent.ToString(Common.defaultIntFormat), " / ",
                    valueModded.ToString(Common.defaultIntFormat),
                    uom);
        }
        public bool HasEnough(float value)
        {
            switch (consumptionMode)
            {
                case ConsumptionMode.HardLimit:
                    return value <= valueCurrent;
                case ConsumptionMode.SoftLimit:
                    return valueCurrent > 0f;
                default:
                    return true;
            }
        }
        public bool IsFull()
        {
            return valueCurrent >= valueModded;
        }
        public void ResetRegenRates()
        {
            regenRate = 0f;
            regenRateFixed = 0f;
        }
        public void EnforceMaxValue()
        {
            if (IsFull())
                SetToPercentage(1);
        }

        /// <summary>
        /// Consumes the amount specified
        /// Uses the consumptionMode property to determine how
        /// </summary>
        /// <returns>True if there was enough available</returns>
        public bool Consume(float amount)
        {
            if (HasEnough(amount))
            {
                valueCurrent -= amount;
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Consume up to the limit specified
        /// </summary>
        /// <returns>Returns left over amount after consumption</returns>
        public float ConsumeLimited(float amount, float limit = 0)
        {
            if ((amount + limit) < valueCurrent)
            {
                valueCurrent -= amount;
                return 0;
            }
            else
            {
                //Calculate the left-overs _before_ changing valueCurrent
                float ret = valueCurrent > limit ?
                    amount - (valueCurrent - limit) :
                    amount;

                //Now it is safe to change valueCurrent
                //NB: This check prevents a semi-invulnerability bug (eg: poison degen could be negated)
                if (valueCurrent > limit)
                    valueCurrent = limit;

                return ret;
            }
        }
        /// <summary>
        /// Used for amounts that may be negative or positive
        /// Positive will simply add to currentValue
        /// Negative will call the consume method
        /// </summary>
        /// <param name="amount">Amount to apply to currentValue</param>
        /// <returns>True if applied successfully</returns>
        public bool ApplyAmount(float amount)
        {
            if (amount < 0)
                return Consume(-amount);
            else
            {
                valueCurrent += amount;
                return true;
            }
        }
        public void SetToPercentage(float percentage = 1f)
        {
            valueCurrent = valueModded * percentage;
        }
        public void SetToFixedAmount(float amount)
        {
            valueCurrent = amount;
        }

        public void CopyFrom(Attribute source, bool copyRuntimeStuff)
        {
            template = source.template;
            valueBase = source.valueBase;
            unitOfMeasure = source.unitOfMeasure;
            consumptionMode = source.consumptionMode;
            regenRate = source.regenRate;
            regenRateFixed = source.regenRateFixed;
            ignoreBaseValue = source.ignoreBaseValue;
            enabled = source.enabled;

            if (copyRuntimeStuff)
            {
                valueTemp = source.valueTemp;
                valueBonus = source.valueTemp;
            }
        }
        #endregion
    }

    public class AttributeSort : System.Collections.Generic.Comparer<Attribute>
    {
        public override int Compare(Attribute x, Attribute y)
        {
            if (x.valueBase != y.valueBase)
                return -x.valueBase.CompareTo(y.valueBase);
            else
                return x.template.GetDisplayName().CompareTo(y.template.GetDisplayName());
        }
    }
}