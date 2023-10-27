using System.Collections.Generic;
using UnityEngine;
using RPGData;
using Events;

[System.Serializable]
[CreateAssetMenu(menuName = "Data/Character Sheet")]
public class CharacterSheet : ScriptableObject
{
    #region Constants
    protected const float
        baseSpeed = 1f,
        baseDashCost = 1f,
        baseDetectionRange = 6f,
        statPointsPerLevel = 0f,
        baseSkillPointsPerLevel = 5f,
        HPBase = 20f,
        MPBase = 0f,
        SPBase = 50f,
        CPBase = 50f,
        APBaseRegen = 10f,
        runMultiplier = 2f,
        sneakMultiplier = 0.5f,
        weightBase = 25f;
    #endregion

    //Properties
    #region Public Properties
    [Header("Identity")]
    public string characterName = "Kat";
    [Header("Stats")]
    public int level = 1;

    public List<Attribute> attributes;
    public List<Attribute> skills;
    public List<Attribute> traits;
    public List<BaseAbility> abilities;

    public BaseStat availableAttributePoints;
    public BaseStat availableSkillPoints;
    [Header("Mechanics")]
    public bool enableSugar = false;
    public bool useDifficultySettings = false;
    #endregion

    #region Protected Properties
    protected Attribute
        _HP = null, //Health Points
        _MP = null, //Mental Points
        _SP = null, //Sugar Points
        _AP = null, //Action Points
        _CP = null; //Contamination Points
    protected float _XP = 0;
    protected Attribute lastSkill = null;
    protected BaseAbility lastAbility = null;
    public List<StatusEffect> statusEffects { get; protected set; }
    [System.NonSerialized]
    protected bool needsRecalc = false;
    #endregion

    #region Accessors
    //Basic types
    public Attribute HP
    {
        get { return _HP; }
    }
    public Attribute MP
    {
        get { return _MP; }
    }
    public Attribute SP
    {
        get { return _SP; }
    }
    public Attribute AP
    {
        get { return _AP; }
    }
    public Attribute CP
    {
        get { return _CP; }
    }
    public float XP
    {
        get { return _XP; }
        set
        {
            _XP = value;
            if (_XP >= XPRequired)
                LevelUp();

            NotifyStatsChanged();
        }
    }
    public float XPRequired
    {
        get { return level * level * 100f; }
    }

    //Equipment
    public Inventory inventory { get; set; }

    //Combat (flat values)
    public ForceMatrix protection { get; protected set; }
    public ForceMatrix damage { get; protected set; }
    public float critRate { get; protected set; }

    //Combat (modifiers)
    public float unarmedDamageModifier { get; protected set; } //get { return((((Str.valueModded * 0.4f) + (Agi.valueModded * 0.1f)) * (DataLookup.GetSkillValue(GetSkill(GameDatabase.sls.unarmed)) * 2.5f)) + bonusUnarmedDamage) * MetagameManager.getDifficultyModifier(useDifficultySettings); }
    public float unarmedCritRateModifier { get; protected set; } //get { return ((Lck.valueModded * 1f) + (Agi.valueModded * 1.5f)) * MetagameManager.getDifficultyModifier(useDifficultySettings); }
    public float meleeWeaponDamageModifier { get; protected set; } //get { return ((Str.valueModded * 0.8f) + (Agi.valueModded * 0.2f) + bonusMeleeWeaponDamage) * DataLookup.GetSkillValue(GetSkill(GameDatabase.sls.melee)) * MetagameManager.getDifficultyModifier(useDifficultySettings); }
    public float meleeWeaponCritRateModifier { get; protected set; } //get { return ((Lck.valueModded * 1f) + (Str.valueModded * 1f)) * MetagameManager.getDifficultyModifier(useDifficultySettings); }
    public float rangedWeaponDamageModifier { get; protected set; }
    public float rangedWeaponCritRateModifier { get; protected set; } //get { return ((Lck.valueModded * 1f) + (Dex.valueModded * 1.5f)) * MetagameManager.getDifficultyModifier(useDifficultySettings); }
    public float rangedWeaponShotAngleModifier { get; protected set; }
    public float protectionModifier { get; protected set; }
    public float staggerChance { get; protected set; }

    //Movement types
    public float dashCost { get; protected set; } //get { return (baseDashCost / Agi.valueModded) * survivalSPModifier; }
    public float speedBase { get; protected set; }
    public float speedRun { get { return speedBase * runMultiplier; } }
    public float speedSneak { get { return speedBase * sneakMultiplier; } }
    public float detectionRange { get; protected set; } //{ get { return baseDetectionRange + Dex.valueModded * 2; } }
    public float stealthRange { get; protected set; } //{ get { return DataLookup.GetSkillValue(GetSkill(GameDatabase.sls.sneak)); } }

    //Misc types
    public float carryWeight { get; protected set; } //{ get { return (weightBase + (Str.valueModded * 5f)); } }
    public float haggleAttempts { get; protected set; } //{ get { return baseHaggleAttempts + Mathf.FloorToInt(GetSkill(GameDatabase.sls.speech).valueModded / 20); } }
    public float haggleSuccessRate { get; protected set; }
    public float lockpickSuccessRate { get; protected set; }
    public float hackingSuccessRate { get; protected set; }
    public float skillPointsPerLevel { get; protected set; } //{ get { return (baseSkillPointsPerLevel + (Int.valueBaseTemp * 2)) * MetagameManager.getDifficultyModifier(useDifficultySettings); } }
    public float learningModifier { get; protected set; }
    public float survivalSPModifier { get; protected set; } //{ get { float skillVal = DataLookup.GetSkillValue(GetSkill(GameDatabase.skillsList.survival), false); return 1 - (0.5f * skillVal); } }
    #endregion

    #region Events
    public System.Action<BaseAbility> OnAbilityAdded;
    public System.Action OnStatsChanged;
    public GameEvent OnLevelUp;

    void NotifyStatsChanged()
    {
        if (OnStatsChanged != null)
            OnStatsChanged();
    }
    #endregion
    
    //Methods
    #region Ability Methods
    public bool HasAbility(BaseAbility ability)
    {
        return abilities.Contains(ability);
    }
    public BaseAbility GetAbility(AbilityTemplate template)
    {
        if (template == null) return null;

        //lastAbility is a caching mechanism for rapid ability finding when doing tasks like using telekinesis which happens ~once per second
        if ((lastAbility == null) || (lastAbility.template != template))
            lastAbility = abilities.Find(x => x.template == template);

        return lastAbility;
    }
    public List<BaseAbility> GetAbilitiesNotOnCooldown(AbilityTemplate.AbilityType[] types)
    {
        List<BaseAbility> list = new List<BaseAbility>();
        if (abilities.Count > 0)
        {
            foreach (BaseAbility foo in abilities)
                if (foo.isReady() && (System.Array.IndexOf(types, foo.template.abilityType) >= 0))
                    list.Add(foo);
        }

        return list;
    }
    public BaseAbility AddAbility(AbilityTemplate template)
    {
        //Make sure no existing ability already exists (if it does, return that instead)
        BaseAbility ability = GetAbility(template);

        //Does ability already exist?
        if (ability == null)
        {
            //Create new ability object
            ability = new BaseAbility(template, true);
            
            //Add new ability to list
            abilities.Add(ability);

            //Fire event
            if (OnAbilityAdded != null)
                OnAbilityAdded(ability);
        }

        return ability;
    }
    public void SortAbilities()
    {
        abilities.Sort(new BaseAbilitySort());
    }
    public SaveAbility[] GetSaveAbilities()
    {
        int count = abilities.Count;
        if (count > 0)
        {
            SaveAbility[] ret = new SaveAbility[count];
            for (int i = 0; i < count; i++)
                ret[i] = new SaveAbility(abilities[i]);

            return ret;
        }
        else
        {
            Debug.LogWarning("Player has no abilities to save?");
            return null;
        }
    }
    public void SetSaveAbilities(SaveAbility[] savedData)
    {
        if (savedData != null)
            foreach (SaveAbility sAbility in savedData)
            {
                //Find existing
                BaseAbility ability = abilities.Find(x => x.template.saveID == sAbility.ID);

                //Create new?
                if (ability == null) ability = new BaseAbility(System.Array.Find(GameDatabase.core.abilities, x => x.saveID == sAbility.ID));

                //Load saved values
                sAbility.unpack(ability);
            }
    }
    public bool LearnAbility(BaseItem book)
    {
        //Get the ability template
        AbilityTemplate template = book.template.ability;

        //Make sure the book is a valid ability book item
        if (template == null)
        {
            Debug.LogWarning("Could not find book ability template!");
            return false;
        }

        //Don't allow non-player Abilities to be learned (these types of books should not be added to the database anyways
        if (template.abilityGroup == AbilityTemplate.AbilityGroup.NonPlayer)
        {
            Debug.LogWarning("A non-player abiltiy book should not exist!");
            return false;
        }

        //Don't allow excluded races to learn this ability
        //if ((template.exclusiveRaces.Count > 0) && (!template.exclusiveRaces.Contains(raceID)))
            //return false;

        //All validation is done, proceed with learning
        BaseAbility ability = GetAbility(book.template.ability);
        if (ability == null)
        {
            //Add ability at level 1
            //addAbility(book.template.ability);

            //Prevent learning new abilities from books.  They must be learned from quests and such.
            Alerter.ShowMessage("You do not understand the contents of this book");
            return true;
        }
        else
        {
            if(ability.bookUsed(book, learningModifier))
            {
                Alerter.ShowMessage("You feel more confident about " + ability.template.name);
                return true;
            }
        }

        return false;
    }
    #endregion
    #region Skill Methods
    public Attribute GetSkill(AttributeTemplate template)
    {
        //lastSkill is a caching mechanism for rapid skill finding when doing tasks like shooting a very high speed weapon
        if ((lastSkill == null) || (lastSkill.template != template))
            lastSkill = skills.Find(x => x.template == template);

        return lastSkill;
    }
    public Attribute GetSkillIndex(int index)
    {
        return skills[index];
    }
    public bool ChangeTempSkill(Attribute skill, float amount)
    {
        //Must have points to spend, or returning points back to pool
        if ((amount < 0) || (availableSkillPoints.valueModded > 0))
        {
            //Reduce amount to what is available
            if (amount > availableSkillPoints.valueModded)
                amount = availableSkillPoints.valueModded;

            //Prevent reducing below last saved value
            if ((amount + skill.valueTemp) < 0)
                amount = (-skill.valueTemp);

            //Ensure that changes do not exceed limits
            if (((skill.valueBaseTemp + amount) <= skill.template.limits.y) &&
                ((skill.valueBaseTemp + amount) >= skill.template.limits.x))
            {
                skill.valueTemp += amount;
                availableSkillPoints.valueTemp -= amount;
                CalcStats();

                return true;
            }
        }

        return false;
    }
    public void SortSkills()
    {
        skills.Sort(new AttributeSort());
    }
    public bool SaveTempSkills()
    {
        if (availableSkillPoints.valueBase < 0)
            Debug.LogError("Available Skill Points has gone below 0!");

        foreach (Attribute skill in skills)
            skill.SaveTempValues();

        availableSkillPoints.SaveTempValues();
        CalcStats();

        return true;
    }
    public SaveSkill[] GetSaveSkills()
    {
        if (skills.Count > 0)
        {
            SaveSkill[] ret = new SaveSkill[skills.Count];
            for (int i = 0; i < skills.Count; i++)
                ret[i] = new SaveSkill(skills[i]);

            return ret;
        }
        else
        {
            Debug.LogError("Player skills missing!");
            return null;
        }
    }
    public void SetSaveSkills(SaveSkill[] saveSkills)
    {
        if (saveSkills != null)
            foreach (SaveSkill sSkill in saveSkills)
            {
                //Find existing
                Attribute skill = skills.Find(x => x.template.saveID == sSkill.ID);

                //Make new?
                if (skill == null) skill = new Attribute(System.Array.Find(GameDatabase.core.skills, x => x.saveID == sSkill.ID));

                //Load saved value
                sSkill.unpack(skill);
            }
    }
    #endregion
    #region Attribute Methods
    public Attribute GetAttribute(AttributeTemplate attributeTemplate)
    {
        return attributes.Find(x => x.template == attributeTemplate);
    }
    public Attribute GetAttribute(int index)
    {
        return attributes[index];
    }
    public bool ChangeTempAttribute(Attribute attribute, float amount)
    {
        //Must have points to spend, or returning points back to pool
        if ((amount < 0) || (availableAttributePoints.valueModded > 0))
        {
            //Reduce amount to what is available
            if (amount > availableAttributePoints.valueModded)
                amount = availableAttributePoints.valueModded;

            //Prevent reducing below last saved value
            if ((amount + attribute.valueTemp) < 0)
                amount = (-attribute.valueTemp);

            //Ensure that changes do not exceed limits
            if (((attribute.valueBaseTemp + amount) <= attribute.template.limits.y) &&
                ((attribute.valueBaseTemp + amount) >= attribute.template.limits.x))
            {
                attribute.valueTemp += amount;
                availableAttributePoints.valueTemp -= amount;
                CalcStats();
                return true;
            }
        }

        return false;
    }
    public bool SaveTempAttributes()
    {
        if(availableAttributePoints.valueBase < 0)
            Debug.LogError("Available Stat Points has gone below 0!");

        foreach (Attribute attribute in attributes)
            attribute.SaveTempValues();

        availableAttributePoints.SaveTempValues();
        CalcStats();

        return true;
    }
    public SaveAttribute[] GetSaveAttributes()
    {
        if (attributes.Count > 0)
        {
            SaveAttribute[] ret = new SaveAttribute[attributes.Count];
            for (int i = 0; i < attributes.Count; i++)
                ret[i] = new SaveAttribute(attributes[i]);

            return ret;
        }
        else
        {
            Debug.LogError("Player stats missing!");
            return null;
        }
    }
    public void SetSaveAttributes(SaveAttribute[] saveAttributes)
    {
        if (saveAttributes != null)
            foreach (SaveAttribute sa in saveAttributes)
            {
                //Find existing
                Attribute attribute = attributes.Find(x => x.template.saveID == sa.ID);
                
                //Make new?
                if (attribute == null) attribute = new Attribute(
                    System.Array.Find(
                        GameDatabase.core.attributes, x => x.saveID == sa.ID));

                //Load saved value
                sa.Unpack(GetAttribute(sa.ID));
            }
    }
    #endregion
    #region Skill/Stat Checking Methods
    public bool StaggerCheck(float damageTaken)
    {
        //TODO: Make formula based on strength, endurance and survival
        return (damageTaken > (HP.valueModded / 3));
    }
    #endregion
    #region Status Effect methods
    /// <summary>
    /// If Status Effect is already active, the duration will be refereshed.
    /// Otherwise, this function adds a new Status Effect object.
    /// </summary>
    public void AddStatusEffect(IStatusEffectSource source)
    {
        //Fetch status effect template from source
        StatusEffectTemplate statusEffect = source.GetTemplate();

        //Don't do anything if there is no status effect assigned
        if (statusEffect == null) return;

        //Don't do anything for sources with no effects
        if (statusEffect.modifiers.Length == 0) return;

        //Check to see if the same Status Effect is already in effect.
        foreach (StatusEffect se in statusEffects)
            if (se.source == source)
            {
                //Found existing Status Effect.  Refresh it's timer.
                se.firedOnce = false;
                se.ResetTimer();
                return;
            }

        StatusEffect newStatusEffect = new StatusEffect(source);
        statusEffects.Add(newStatusEffect);
        CalcStats();

        bool instantEffectsOnly = true;

        //Apply instant effects
        foreach (StatRule modifier in newStatusEffect.modifiers)
        {
            //float amount = (modifier.power * DataLookup.GetSkillModifier(skills.ToArray(), statusEffect.skillModifiers));
            float amount = (modifier.percentage);

            switch (modifier.affectedStat)
            {
                case StatTypes.hP: HP.ApplyAmount(amount); break;
                case StatTypes.mP: MP.ApplyAmount(amount); break;
                case StatTypes.sP: SP.ApplyAmount(amount); break;
                case StatTypes.aP: AP.ApplyAmount(amount); break;
                case StatTypes.cP: CP.ApplyAmount(amount); break;
                default: instantEffectsOnly = false; break;
            }
        }

        //Instant effect-only Status Effect don't need to linger
        if (instantEffectsOnly)
            RemoveStatusEffect(source, false);
    }
    /// <summary>
    /// Removes any Status Effect that is the result of using an item
    /// </summary>
    /// <param name="itemID">ID of the item used to get the Status Effect</param>
    /// <param name="doRecalc">Weather or not to do a full character sheet recalculation</param>
    public void RemoveStatusEffect(IStatusEffectSource source, bool doRecalc = true)
    {
        foreach (StatusEffect effect in statusEffects)
            if (effect.source == source)
            {
                statusEffects.Remove(effect);
                needsRecalc = doRecalc;
                return;
            }
    }
    public void RemoveAllStatusEffects(bool doRecalc)
    {
        while (statusEffects.Count > 0)
            RemoveStatusEffect(statusEffects[statusEffects.Count - 1].source, false);

        if(doRecalc)
            CalcStats();
    }
    /// <summary>
    /// Checks timeStamps and removes any that have expired
    /// </summary>
    public void HandleStatusEffects()
    {
        bool doAgain = false;
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.isExpired())
            {
                RemoveStatusEffect(effect.source);
                doAgain = true;
                break;
            }
        }

        if (doAgain)
            HandleStatusEffects();
        else
            if (needsRecalc)
            {
                CalcStats();
                needsRecalc = false;
            }
    }
    #endregion
    #region Startup Methods
    int AttributeLimitDiff(Vector2 limit, Attribute attribute)
    {
        if (attribute.valueBaseTemp < limit.x)
            return Mathf.RoundToInt(attribute.valueBaseTemp - limit.x);

        if(attribute.valueBaseTemp > limit.y)
            return Mathf.RoundToInt(attribute.valueBaseTemp - limit.y);

        return 0;
    }
    public CharacterSheet()
    {
        //Initialize level
        level = 1;

        //Initialize stats, skills and abilities
        //InitStats();
        //InitSkills();
        //InitAbilities();
        if (statusEffects == null) statusEffects = new List<StatusEffect>();

        //Initialize Other stats
        if (_HP == null) _HP = new Attribute(1f);
        if (_MP == null) _MP = new Attribute(0f, Attribute.ConsumptionMode.HardLimit);
        if (_SP == null) _SP = new Attribute(1f);
        if (_AP == null) _AP = new Attribute(1f, Attribute.ConsumptionMode.SoftLimit);
        if (_CP == null) _CP = new Attribute(0f);

        //Link AP Max value to SP current value
        SP.OnCurrentValueChanged += UpdateAP_BaseValue;

        //Fire event
        NotifyStatsChanged();
    }
    void CopyAttributesList(List<Attribute> source, List<Attribute> target, bool copyRuntimeData)
    {
        target.Clear();
        foreach (Attribute a in source)
        {
            Attribute aNew = new Attribute();
            aNew.CopyFrom(a, copyRuntimeData);
            target.Add(aNew);
        }
    }
    public void CopyFrom(CharacterSheet source, bool copyRuntimeData)
    {
        characterName = source.characterName;
        level = source.level;

        CopyAttributesList(source.attributes, attributes, copyRuntimeData);
        CopyAttributesList(source.skills, skills, copyRuntimeData);
        CopyAttributesList(source.traits, traits, copyRuntimeData);
        abilities.Clear();
        foreach (BaseAbility a in source.abilities)
        {
            BaseAbility aNew = new BaseAbility(a.template);
            aNew.CopyFrom(a, copyRuntimeData);
            abilities.Add(aNew);
        }

        availableAttributePoints = source.availableAttributePoints;
        availableSkillPoints = source.availableSkillPoints;
        enableSugar = source.enableSugar;
        useDifficultySettings = source.useDifficultySettings;

        if (copyRuntimeData)
        {
            _HP.CopyFrom(source.HP, true);
            _MP.CopyFrom(source.MP, true);
            _SP.CopyFrom(source.SP, true);
            _AP.CopyFrom(source.AP, true);
            _CP.CopyFrom(source.CP, true);
            _XP = source.XP;

            //TODO?  Or copy with no status effects?
            //statusEffects
        }

        //No matter what, a recalc will be needed
        needsRecalc = true;
    }

    public void OnDeserialized()
    {
        //Create skills list if needed
        if (skills == null) skills = new List<Attribute>();

        //Check to see if this savegame was just upgraded from an old version
        if (SP.valueBase == 0)
        {
            //Give a free topup to get things back on track
            CalcStats();
            FillUp();
        }
    }
    #endregion
    #region Upkeep Methods
    void ResetStats()
    {
        damage = new ForceMatrix(0f);
        protection = new ForceMatrix(0f);
        critRate = 0f;

        //Combat types (offense)
        unarmedDamageModifier = 0f;
        unarmedCritRateModifier = 0f;
        meleeWeaponDamageModifier = 0f;
        meleeWeaponCritRateModifier = 0f;
        rangedWeaponDamageModifier = 0f;
        rangedWeaponCritRateModifier = 0f;

        //Combat types (defense)
        protectionModifier = 0f;
        staggerChance = 0f;

        //Movement types
        dashCost = 0f;
        speedBase = baseSpeed;
        //speedRun = speedBase * runMultiplier;
        //speedSneak = speedBase * sneakMultiplier;
        detectionRange = baseDetectionRange;
        stealthRange = 0f;

        //Misc types
        carryWeight = weightBase;
        haggleAttempts = 0;
        lockpickSuccessRate = 0f;
        hackingSuccessRate = 0f;
        skillPointsPerLevel = baseSkillPointsPerLevel;
    }
    void ResetBonuses()
    {
        //StatEx
        HP.ResetBonuses();
        MP.ResetBonuses();
        SP.ResetBonuses();
        AP.ResetBonuses();
        CP.ResetBonuses();

        foreach (Attribute attribute in attributes)
            attribute.ResetBonuses();
    }
    void CalcBaseStats()
    {
        //Used to maintain % of current value when max value changes
        float beforeRatio;

        //Should these stats be adjusted based on game difficulty chosen?
        float difficultyModifier = GameDatabase.sGameSettings
            .GetDifficultyModifier(useDifficultySettings);

        //Calc Health
        if (HP.valueCurrent <= 0) beforeRatio = 1;
        else beforeRatio = HP.valueCurrent / HP.valueModded;

        HP.valueBase = HPBase * difficultyModifier;
        HP.EnforceMaxValue();
        HP.SetToFixedAmount(HP.valueModded * beforeRatio);

        //Calc Mana
        beforeRatio = MP.valueCurrent / MP.valueModded;
        MP.valueBase = MPBase * difficultyModifier;
        MP.EnforceMaxValue();
        MP.SetToFixedAmount(MP.valueModded * beforeRatio);

        //Calc Sugar
        //Constant degen (usually for player only)
        SP.regenRateFixed += enableSugar ? (-0.05f * survivalSPModifier) : 0f;
        beforeRatio = SP.valueCurrent / SP.valueModded;
        SP.valueBase = SPBase;
        SP.EnforceMaxValue();
        SP.SetToFixedAmount(SP.valueModded * beforeRatio);

        //Calc Action Points (Stamina) regen
        AP.regenRateFixed += APBaseRegen;

        //Calc Contamination
        if (CP.valueCurrent <= 0) beforeRatio = 0;
        else beforeRatio = CP.valueCurrent / CP.valueModded;
        CP.valueBase = CPBase * difficultyModifier;
        CP.EnforceMaxValue();
        CP.SetToFixedAmount(CP.valueModded * beforeRatio);
    }
    void CalcStatRule(StatRule rule, float statSource)
    {
        switch (rule.affectedStat)
        {
            //Basic types
            case StatTypes.hP_Regen: HP.regenRateFixed += rule.GetAmountToAdd(HP.regenRateFixed, statSource); break;
            case StatTypes.mP_Regen: MP.regenRateFixed += rule.GetAmountToAdd(MP.regenRateFixed, statSource); break;
            case StatTypes.sP_Regen: SP.regenRateFixed += rule.GetAmountToAdd(SP.regenRateFixed, statSource); break;
            case StatTypes.aP_Regen: AP.regenRateFixed += rule.GetAmountToAdd(AP.regenRateFixed, statSource); break;
            case StatTypes.cP_Regen: CP.regenRateFixed += rule.GetAmountToAdd(CP.regenRateFixed, statSource); break;
            case StatTypes.hP_Max: HP.valueBonus += rule.GetAmountToAdd(HP.valueBase, statSource); break;
            case StatTypes.mP_Max: MP.valueBonus += rule.GetAmountToAdd(MP.valueBase, statSource); break;
            case StatTypes.sP_Max: SP.valueBonus += rule.GetAmountToAdd(SP.valueBase, statSource); break;
            case StatTypes.aP_Max: AP.valueBonus += rule.GetAmountToAdd(AP.valueBase, statSource); break;
            case StatTypes.cP_Max: CP.valueBonus += rule.GetAmountToAdd(CP.valueBase, statSource); break;

            //Item types
            //itemStatAffect = 100,
            //itemDamage = 101,
            //itemCritRate = 102,
            //itemProtection = 103,

            //itemCraftQuantity = 110,
            //itemCraftMaterialCost = 111,
            //itemDeconstructMaterialReturn = 112,

            //Combat types (defense) 
            /*
            unarmedDamagePhysical = 150,
            unarmedDamageChemical = 151,
            unarmedDamageEnergy = 153,
            unarmedDamageBiological = 154,
            itemDamagePhysical = 155,
            itemDamageChemical = 156,
            itemDamageEnergy = 157,
            itemDamageBiological = 158,

            physicalProtection = 160,
            chemicalProtection = 161,
            energyProtection = 162,
            biologicalProtection = 163,
            itemPhysicalProtection = 164,
            itemChemicalProtection = 165,
            itemEnergyProtection = 166,
            itemBiologicalProtection = 167,
                    */
            case StatTypes.staggerChance: staggerChance += rule.GetAmountToAdd(staggerChance, statSource); break;

            //Movement types
            case StatTypes.speed: speedBase += rule.GetAmountToAdd(speedBase, statSource); break;
            //case StatTypes.runSpeed: speedRun += rule.GetAmountToAdd(speedRun, statSource); break;
            //case StatTypes.sneakSpeed: speedSneak += rule.GetAmountToAdd(speedSneak, statSource); break;
            case StatTypes.dashCost: dashCost += rule.GetAmountToAdd(dashCost, statSource); break;
            case StatTypes.detectionRange: detectionRange += rule.GetAmountToAdd(detectionRange, statSource); break;
            case StatTypes.stealthRange: stealthRange += rule.GetAmountToAdd(stealthRange, statSource); break;

            //Misc types
            case StatTypes.carryWeight: carryWeight += rule.GetAmountToAdd(carryWeight, statSource); break;
            case StatTypes.haggleAttempts: haggleAttempts += rule.GetAmountToAdd(haggleAttempts, statSource); break;
            case StatTypes.haggleSuccessRate: haggleSuccessRate += rule.GetAmountToAdd(haggleSuccessRate, statSource); break;
            case StatTypes.lockpickSuccessRate: lockpickSuccessRate += rule.GetAmountToAdd(lockpickSuccessRate, statSource); break;
            case StatTypes.hackingSuccessRate: hackingSuccessRate += rule.GetAmountToAdd(hackingSuccessRate, statSource); break;
            case StatTypes.skillPointsPerLevel: skillPointsPerLevel += rule.GetAmountToAdd(skillPointsPerLevel, statSource); break;

                //TODO: Add the rest of the statTypes!
        }
    }
    void CalcAttributes()
    {
        //Apply Status Effects
        foreach (Attribute attribute in attributes)
            foreach (StatRule statRule in attribute.template.statRules)
                CalcStatRule(statRule, attribute.valueModded);
    }
    void CalcStatusEffects()
    {
        //Apply Status Effects
        foreach (StatusEffect effect in statusEffects)
            foreach (StatRule modifier in effect.modifiers)
                CalcStatRule(modifier, 1f);
    }
    public void CalcStats()
    {
        ResetStats();

        //Reset all bonuses
        ResetBonuses();

        CalcBaseStats();
        CalcAttributes();
        CalcStatusEffects();

        //Flat combat values
        CalcProtection();
        CalculateDamage();

        //Haggle testing
        //haggleSuccessRate = 50f;

        //Fire events
        NotifyStatsChanged();
    }
    
    //Caching calculations
    protected virtual void CalcProtection()
    {
        //This is just a placeholder, plz fix!
        protection = new ForceMatrix(protectionModifier);
    }
    protected virtual void CalculateDamage()
    {
        //Armed
        if ((inventory) && (inventory.equippedWeapon != null))
        {
            //critRate = inventory.equippedWeapon.template.crit
            //Apply damage modification to the damage matrix and store in cache
            damage = inventory.equippedWeapon.template.damageMatrix *
                (inventory.equippedWeapon.template.isMeleeWeapon() ?
                    meleeWeaponDamageModifier :
                    rangedWeaponDamageModifier);
        }
        //Unarmed
        else
        {
            //Default
            damage = new ForceMatrix(1f);
        }
    }

    private void UpdateAP_BaseValue(float newValue, float delta, float overflow)
    {
        AP.valueBase = newValue;
        AP.EnforceMaxValue();
    }
    /// <summary>
    /// Updates character sheet's realtime counters (regen, etc)
    /// Should be called every frame
    /// </summary>
    /// <param name="deltaTime">Time since last frame</param>
    public void Tick(float deltaTime, bool regenAP)
    {
        DoRegen(deltaTime, regenAP);

        if (abilities != null)
        {
            //Updates ability cooldowns
            foreach (BaseAbility ability in abilities)
                ability.tick(deltaTime);
        }
    }
    void DoRegen(float deltaTime, bool regenAP)
    {
        //HP Regen
        if (HP != null) HP.DoRegen(deltaTime);
        
        //MP Regen
        if (MP != null) MP.DoRegen(deltaTime);

        //CP Over time
        if (CP != null) CP.DoRegen(deltaTime);

        //SP Regen
        if ((enableSugar) && (SP != null))
        {
            //Degen HP once your reach 0 Sugar
            float leftOver = SP.DoRegen(deltaTime);

            //When sugar reaches 0, consume HP instead
            HP.ConsumeLimited(leftOver, 1f);
            /*
            if ((HP.valueCurrent - leftOver) > 1f)
                HP.valueCurrent -= leftOver;

            //Do not allow death from this mechanic
            else
            {
                //NB: This check prevents a semi-invulnerability bug (eg: poison degen could be negated)
                if (HP.valueCurrent > 1)    
                    HP.valueCurrent = 1;
            }*/
        }

        //AP regen
        if (regenAP && AP != null)
            AP.DoRegen(deltaTime);
    }
    public void FillUp(float multiplier = 1f, float contaminationPercent = 0f)
    {
        HP.SetToPercentage(multiplier);
        MP.SetToPercentage(multiplier);
        SP.SetToPercentage(multiplier);
        AP.SetToPercentage(multiplier);
        CP.SetToPercentage(contaminationPercent);

        NotifyStatsChanged();
    }
    void LevelUp()
    {
        _XP -= XPRequired;
        level += 1;
        availableAttributePoints.valueBase += statPointsPerLevel;
        availableSkillPoints.valueBase += skillPointsPerLevel;
        CalcStats();

        //Fire event
        OnLevelUp.Raise();
    }
    #endregion
}