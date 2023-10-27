using UnityEngine;
using UnityEditor;
using RPGData;
using System.Collections.Generic;

[CustomEditor(typeof(CharacterSheet))]
[CanEditMultipleObjects]
public class CharacterSheetEditor : Editor
{
    CharacterSheet copySource = null;
    CharacterSheet me;

    void OnEnable()
    {
        me = ((CharacterSheet)serializedObject.targetObject);
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        copySource = (CharacterSheet)EditorGUILayout.ObjectField(
            "Source", 
            copySource, 
            typeof(CharacterSheet), 
            false);
        if (GUILayout.Button("Copy")) CopyCharacterSheet(copySource);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Actions")) DisplayActionsStatus();
        if (GUILayout.Button("Reset Skills")) AddAllSkills();
        EditorGUILayout.EndHorizontal();
    }

    void AddAllSkills()
    {
        me.skills.Clear();

        var skillGuids = AssetDatabase.FindAssets("t:AttributeTemplate");

        foreach (string skillGuid in skillGuids)
        {
            var pSkill = AssetDatabase.GUIDToAssetPath(skillGuid);
            AttributeTemplate skill = (AttributeTemplate)AssetDatabase.LoadAssetAtPath(pSkill, typeof(AttributeTemplate));
            if (skill.isSkill) me.skills.Add(new Attribute() {
                template = skill,
                valueBase = 1,
                enabled = false,
            });
        }

        EditorUtility.SetDirty(target);
    }
    void DisplayActionsStatus()
    {
        Debug.Log(me.OnAbilityAdded);
        Debug.Log(me.OnStatsChanged);
    }
    void CopyCharacterSheet(CharacterSheet source)
    {
        if (!source) return;

        me.CopyFrom(source, false);

        EditorUtility.SetDirty(target);

        Debug.Log("Copy successful");
    }
}