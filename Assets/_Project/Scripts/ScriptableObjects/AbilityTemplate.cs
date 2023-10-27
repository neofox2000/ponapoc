using UnityEngine;

[CreateAssetMenu(menuName = "Templates/Ability")]
public class AbilityTemplate : ScriptableObject, IStatusEffectSource
{
    [System.Serializable]
    public class AbilityConfig
    {
        public AbilityTemplate ability;
        public int animationKey;
    }

    /// <summary>
    /// Generic = Everyone
    /// NonPlayer = NPCs and Enemies only
    /// Player = Player only
    /// </summary>
    public enum AbilityGroup { Generic, NonPlayer, Player }
    public enum AbilityType { Movement, Defensive, Offensive }

    public int saveID;
    public string displayName;
    public Color32 color;
    public Sprite icon;
    [Tooltip("Gameobject containing the script, special effects, spawners, etc for the ability execution phase")]
    public GameObject payload;
    [Tooltip("Should the payload use the pooling system?\nUse this for payloads that spawn often\neg: rapid-fire or many enemies using the same ability in the same space")]
    public bool poolPayload;
    [TextArea(3, 10)]
    public string description;

    [Header("Exclusivity")]
    [Tooltip("Which type of entity can use this ability")]
    public AbilityGroup abilityGroup = AbilityGroup.Generic;
    [Tooltip("What type of ability this is; determines the slot it can be put into")]
    public AbilityType abilityType = AbilityType.Offensive;

    [Header("Statistics")]
    [Tooltip("The level at which BOTH theory and practice can no longer be increased")]
    public float maxLevel;
    [Tooltip("How strong the ability is")]
    public float power;
    [Tooltip("How long the ability's effect should last (if applicable)")]
    public float duration;
    [Tooltip("MP cost to use this ability")]
    public float mpcost;
    [Tooltip("AP cost to use this ability")]
    public float apcost;
    [Tooltip("How fast the ability moves/shoots/etc.")]
    public float speed;
    [Tooltip("How far away the ability can reach")]
    public float range;
    [Tooltip("How long before ability can be used again")]
    public float cooldown;
    [Tooltip("What types of damage this ability applies/absorbs (if applicable)")]
    public ForceMatrix damageTypes;
    [Tooltip("Status Effects applied when item is used/equipped")]
    public StatusEffectTemplate statusEffect;

    [Header("Growth")]
    [Tooltip("Base Practice XP value awarded for each use of this ability")]
    public float pxpPerUse;
    [Tooltip("Base Theory XP value awarded for each book read")]
    public float txpPerBook;

    //IStatusEffectSource Methods
    StatusEffectTemplate IStatusEffectSource.GetTemplate()
    {
        return statusEffect;
    }

    //Quick Methods
    public string GetReadout()
    {
        return string.Concat(
            name, "\n\n",
            "Cooldown: " + cooldown, "\n",
            "MP Cost: " + mpcost, "\n",
            "AP Cost: " + apcost, "\n\n",
            description);
    }
}