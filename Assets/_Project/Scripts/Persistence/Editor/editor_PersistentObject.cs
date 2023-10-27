using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PersistentObject))]
public class editor_PersistentObject : Editor
{
    protected SerializedProperty id;

    void OnEnable()
    {
        id = serializedObject.FindProperty("id");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(id);
            if (GUILayout.Button("Generate"))
            {
                //Find the highest id
                PersistentObject[] objects = FindObjectsOfType<PersistentObject>();
                int highest = 0;
                foreach (PersistentObject lo in objects)
                    if (lo.id > highest)
                        highest = lo.id;

                //Increment over highest
                id.intValue = highest + 1;
            }
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}