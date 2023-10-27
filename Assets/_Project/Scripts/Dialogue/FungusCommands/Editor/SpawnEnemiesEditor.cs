using UnityEditor;
using UnityEngine;

namespace Fungus.EditorUtils
{
    [CustomEditor(typeof(SpawnEnemies))]
    public class SpawnEnemiesEditor : CallEditor
    {
        protected SerializedProperty enemyGroupProp;

        public override void OnEnable()
        {
            //base.OnEnable();
            if (NullTargetCheck()) // Check for an orphaned editor instance
                return;

            targetFlowchartProp = serializedObject.FindProperty("targetFlowchart");
            targetBlockProp = serializedObject.FindProperty("targetBlock");
            startIndexProp = serializedObject.FindProperty("startIndex");
            callModeProp = serializedObject.FindProperty("callMode");

            enemyGroupProp = serializedObject.FindProperty("enemyGroup");
        }

        void drawbase()
        {
            Call t = target as Call;

            Flowchart flowchart = null;
            if (targetFlowchartProp.objectReferenceValue == null)
            {
                flowchart = (Flowchart)t.GetFlowchart();
            }
            else
            {
                flowchart = targetFlowchartProp.objectReferenceValue as Flowchart;
            }

            EditorGUILayout.PropertyField(targetFlowchartProp);

            if (flowchart != null)
            {
                BlockEditor.BlockField(targetBlockProp,
                                       new GUIContent("Target Block", "Block to call"),
                                       new GUIContent("<None>"),
                                       flowchart);

                EditorGUILayout.PropertyField(startIndexProp);
            }

            EditorGUILayout.PropertyField(callModeProp);
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(enemyGroupProp);

            drawbase();

            serializedObject.ApplyModifiedProperties();
        }
    }
}