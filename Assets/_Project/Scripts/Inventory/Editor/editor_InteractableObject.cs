using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(InteractableObject))]
public class editor_InteractableObject : Editor
{
    public class Properties
    {
        //Interactions
        public SerializedProperty objectStateAnimator;
        public SerializedProperty automatic;
        public SerializedProperty showText;
        public SerializedProperty floatingTextOffset;
        public SerializedProperty floatingTextMessage;
        public SerializedProperty progressBar;

        //Access Methods
        public SerializedProperty acessState;
        public SerializedProperty keyItem;
        public SerializedProperty keyConsumed;
        public SerializedProperty canLockpick;
        public SerializedProperty skillRequired;
        public SerializedProperty skillReq;
        public SerializedProperty minSkillRequired;
        public SerializedProperty lockedMessage;
        public SerializedProperty unlockedMessage;

        //Sounds
        public SerializedProperty activationSound;
        public SerializedProperty blockedSound;
        public SerializedProperty unlockedSound;
        public SerializedProperty lockpickSuccessSound;
        public SerializedProperty lockpickFailSound;
        public SerializedProperty lockpickCancelSound;

        public Properties(SerializedObject serializedObject)
        {
            //Interactions
            objectStateAnimator = serializedObject.FindProperty("objectStateAnimator");
            automatic = serializedObject.FindProperty("automatic");
            showText = serializedObject.FindProperty("showText");
            floatingTextOffset = serializedObject.FindProperty("floatingTextOffset");
            floatingTextMessage = serializedObject.FindProperty("floatingTextMessage");
            progressBar = serializedObject.FindProperty("progressBar");

            //Access Methods
            acessState = serializedObject.FindProperty("accessState");
            keyItem = serializedObject.FindProperty("keyItem");
            keyConsumed = serializedObject.FindProperty("keyConsumed");
            canLockpick = serializedObject.FindProperty("canLockpick");
            skillReq = serializedObject.FindProperty("skillReq");
            minSkillRequired = serializedObject.FindProperty("minSkillRequired");
            lockedMessage = serializedObject.FindProperty("lockedMessage");
            unlockedMessage = serializedObject.FindProperty("unlockedMessage");

            //Sounds
            activationSound = serializedObject.FindProperty("activationSound");
            blockedSound = serializedObject.FindProperty("blockedSound");
            unlockedSound = serializedObject.FindProperty("unlockedSound");
            lockpickSuccessSound = serializedObject.FindProperty("lockpickSuccessSound");
            lockpickFailSound = serializedObject.FindProperty("lockpickFailSound");
            lockpickCancelSound = serializedObject.FindProperty("lockpickCancelSound");
        }
    }

    protected Properties properties;


    protected virtual void OnEnable()
    {
        properties = new Properties(serializedObject);
    }

    public override void OnInspectorGUI()
    {
        //Interactions
        EditorGUILayout.PropertyField(properties.objectStateAnimator);
        EditorGUILayout.PropertyField(properties.automatic);
        EditorGUILayout.PropertyField(properties.showText);
        if (properties.showText.boolValue)
        {
            EditorGUILayout.PropertyField(properties.floatingTextOffset);
            EditorGUILayout.PropertyField(properties.floatingTextMessage);
        }


        //Access Methods
        EditorGUILayout.PropertyField(properties.acessState);
        InteractableObject.AccessState accessState = (InteractableObject.AccessState)properties.acessState.intValue;
        if (accessState != InteractableObject.AccessState.Open)
        {
            EditorGUILayout.PropertyField(properties.keyItem);
            EditorGUILayout.PropertyField(properties.keyConsumed);
            EditorGUILayout.PropertyField(properties.canLockpick);
            if (properties.canLockpick.boolValue)
            {
                EditorGUILayout.PropertyField(properties.skillReq);
                EditorGUILayout.PropertyField(properties.minSkillRequired);
                EditorGUILayout.PropertyField(properties.lockedMessage);
                EditorGUILayout.PropertyField(properties.unlockedMessage);
                EditorGUILayout.PropertyField(properties.progressBar);
            }
        }


        //Sounds
        EditorGUILayout.PropertyField(properties.activationSound);
        if (accessState != InteractableObject.AccessState.Open)
        {
            EditorGUILayout.PropertyField(properties.unlockedSound);
            EditorGUILayout.PropertyField(properties.blockedSound);
            EditorGUILayout.PropertyField(properties.lockpickSuccessSound);
            EditorGUILayout.PropertyField(properties.lockpickFailSound);
            EditorGUILayout.PropertyField(properties.lockpickCancelSound);
        }

        //Save changes
        serializedObject.ApplyModifiedProperties();
    }
}