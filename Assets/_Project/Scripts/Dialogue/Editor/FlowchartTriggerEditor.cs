using UnityEditor;
using UnityEngine;

namespace Fungus.EditorUtils
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FlowchartTrigger))]
    public class FlowchartTriggerEditor : Editor
    {
        protected SerializedProperty flowchartProp;
        protected SerializedProperty blockProp;
        protected SerializedProperty triggerProp;
        protected SerializedProperty triggerDelayProp;
        protected SerializedProperty cleanupActionProp;

        protected virtual void OnEnable()
        {
            flowchartProp = serializedObject.FindProperty("flowchart");
            blockProp = serializedObject.FindProperty("block");
            triggerProp = serializedObject.FindProperty("trigger");
            triggerDelayProp = serializedObject.FindProperty("triggerDelay");
            cleanupActionProp = serializedObject.FindProperty("cleanupAction");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField("Trigger Settings");
            EditorGUILayout.PropertyField(triggerProp);
            EditorGUILayout.PropertyField(triggerDelayProp);
            EditorGUILayout.PropertyField(cleanupActionProp);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dialogue Settings");
            EditorGUILayout.PropertyField(flowchartProp);

            if (flowchartProp.objectReferenceValue != null)
            {
                Flowchart flowchart = null;
                flowchart = flowchartProp.objectReferenceValue as Flowchart;

                string selectedBlockName = blockProp.stringValue;

                int index = 0;
                var blocks = flowchart.GetComponents<Block>();
                string[] blockNames = new string[blocks.Length + 1];
                blockNames[0] = "<None>";

                for (int i = 0; i < blocks.Length; i++)
                {
                    //Store the name in the array
                    blockNames[i+1] = blocks[i].BlockName;

                    //Store the index for selected value
                    if (selectedBlockName == blockNames[i+1])
                        index = i + 1;
                }

                //Display popup and get new value
                index = EditorGUILayout.Popup(blockProp.displayName, index, blockNames);

                //Store results of selection
                if (index > 0)
                    blockProp.stringValue = blockNames[index];
                else
                    blockProp.stringValue = string.Empty;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
