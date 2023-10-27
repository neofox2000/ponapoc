using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(LocationPortal))]
public class editor_LocationPortal : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("template"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directionToFace"));

        SerializedProperty portalTypeProp = serializedObject.FindProperty("portalType");
        EditorGUILayout.PropertyField(portalTypeProp);
        if (portalTypeProp.intValue == 1)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portalTarget"));

        serializedObject.ApplyModifiedProperties();
    }
}