using UnityEngine;
using RPGData;
//using GameDB;

[CreateAssetMenu(menuName = "Templates/Status Effect")]
public class StatusEffectTemplate : ScriptableObject
{
    [Header("Display")]
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;
    public Color32 iconColor = Color.white;

    [Header("Effects")]
    public float duration;
    public StatRule[] modifiers;
    //public GameDB.StatModifier[] statModifiers;
    //public GameDB.SkillModifier[] skillModifiers;
}