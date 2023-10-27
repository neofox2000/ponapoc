using UnityEditor;
using UnityEngine;
using RPGData;

[CustomPropertyDrawer(typeof(Attribute), true)]
public class AttributeDrawer : PropertyDrawer
{
    const float fieldHeight = 16f;
    const float paddingY = 2f;

    public static readonly string[] propList = {
        "template",
        "valueBase",
        "unitOfMeasure",
        "enabled",
        "consumptionMode",
        "regenRate",
        "regenRateFixed",
        "ignoreBaseValue",
    };    

    int GetPropertyCountToShow(SerializedProperty property)
    {
        if (property.isExpanded)
        {
            int uomOffset = 0;
            AttributeTemplate template = (AttributeTemplate)property.FindPropertyRelative(propList[0]).objectReferenceValue;
            if ((template) && (template.unitOfMeasure))
                uomOffset = -1;

            if (property.FindPropertyRelative(propList[3]).boolValue)
                return propList.Length + uomOffset;
            else
                return 4 + uomOffset;
        }
        else return 0;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 
            base.GetPropertyHeight(property, label) + 
            (fieldHeight * GetPropertyCountToShow(property));

        /*
        if (property.isExpanded)
        {
            int uomOffset = property.FindPropertyRelative(propList[0]).objectReferenceValue ? -1 : 0;

            if (property.FindPropertyRelative("enabled").boolValue)
                return baseHeight + uomOffset + (fieldHeight * propList.Length);
            else
                return baseHeight + uomOffset + (fieldHeight * 4);
        }
        else
            return baseHeight;
            */
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty templateProp = property.FindPropertyRelative(propList[0]);

        if (templateProp.objectReferenceValue == null)
        {
            property.isExpanded = false;
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.PropertyField(position, templateProp);
            return;
        }

        // Draw label
        float unindentedX = position.x;
        if ((templateProp.objectReferenceValue != null) && (label.text.Contains("Element"))) label.text = (templateProp.objectReferenceValue as RPGData.AttributeTemplate).GetDisplayName();
        Rect labelPos = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var foldoutRect = new Rect(unindentedX, position.y, position.width - labelPos.width, fieldHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);

        //Draw first property
        int i = 0;
        EditorGUI.PropertyField(
            new Rect(labelPos.x, labelPos.y + (fieldHeight * i) + (paddingY * i), labelPos.width, fieldHeight),
            templateProp,
            GUIContent.none);

        //Don't do anything else unless foldout is expanded
        if (!property.isExpanded) return;

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel++;

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        AttributeTemplate template = (AttributeTemplate)property.FindPropertyRelative(propList[0]).objectReferenceValue;
        bool showUOM = (template == null) || (template.unitOfMeasure == null);
        bool useExtended = property.FindPropertyRelative(propList[3]).boolValue;

        int drawCount = 1;
        for (i = 1; i < propList.Length; i++)
        {
            bool drawProp;
            switch(i)
            {
                case 0:
                case 1: drawProp = true; break;
                case 2: drawProp = showUOM; break;
                case 3: drawProp = true; break;
                default: drawProp = useExtended; break;
            }

            if (drawProp)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + (fieldHeight * drawCount) + (paddingY * drawCount), position.width, fieldHeight),
                    property.FindPropertyRelative(propList[i]));

                drawCount++;
            }
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;
    }
}