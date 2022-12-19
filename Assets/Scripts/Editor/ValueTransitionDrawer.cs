using UnityEditor;
using UnityEngine;
using static ValueTransition;

[CustomPropertyDrawer(typeof(ValueTransition))]
public class ValueTransitionDrawer : PropertyDrawer
{
    private SerializedProperty GetSetterEvent(SerializedProperty property)
    {
        return (ValueType)property.FindPropertyRelative("valueType").enumValueIndex switch
        {
            ValueType.Vector2 => property.FindPropertyRelative("_vector2Setter"),
            ValueType.Vector3 => property.FindPropertyRelative("_vector3Setter"),
            ValueType.Color => property.FindPropertyRelative("_colorSetter"),
            ValueType.Quaternion => property.FindPropertyRelative("_quaternionSetter"),
            ValueType.Bool => property.FindPropertyRelative("_boolSetter"),
            _ => property.FindPropertyRelative("_floatSetter"),
        };
    }

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        pos.height = height;

        label = EditorGUI.BeginProperty(pos, label, property);

        EditorGUI.PropertyField(pos, property, label);
        pos.y += height + spacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            var valueType = (ValueType)property.FindPropertyRelative("valueType").enumValueIndex;

            var setterProp = GetSetterEvent(property);
            EditorGUI.PropertyField(pos, setterProp);
            pos.y += EditorGUI.GetPropertyHeight(setterProp) + spacing;

            var startValProp = property.FindPropertyRelative("startValue");
            var endValProp = property.FindPropertyRelative("endValue");

            var startVal = startValProp.vector4Value;
            var endVal = endValProp.vector4Value;

            EditorGUI.BeginChangeCheck();
            switch (valueType)
            {
                case ValueType.Float:
                    startVal[0] = EditorGUI.FloatField(pos, startValProp.displayName, startVal[0]);
                    pos.y += height + spacing;

                    endVal[0] = EditorGUI.FloatField(pos, endValProp.displayName, endVal[0]);
                    break;
                case ValueType.Vector2:
                    startVal = EditorGUI.Vector2Field(pos, startValProp.displayName, startVal);
                    pos.y += height + spacing;

                    endVal = EditorGUI.Vector2Field(pos, endValProp.displayName, endVal);
                    break;
                case ValueType.Vector3:
                    startVal = EditorGUI.Vector3Field(pos, startValProp.displayName, startVal);
                    pos.y += height + spacing;

                    endVal = EditorGUI.Vector3Field(pos, endValProp.displayName, endVal);
                    break;
                case ValueType.Color:
                    startVal = EditorGUI.ColorField(pos, startValProp.displayName, startVal);
                    pos.y += height + spacing;

                    endVal = EditorGUI.ColorField(pos, endValProp.displayName, endVal);
                    break;
                case ValueType.Quaternion:
                    EditorGUI.BeginChangeCheck();
                    var startRot = EditorGUI.Vector3Field(pos, startValProp.displayName, Utils.QuaternionFromVector4(startVal).eulerAngles);
                    pos.y += height + spacing;

                    var endRot = EditorGUI.Vector3Field(pos, endValProp.displayName, Utils.QuaternionFromVector4(endVal).eulerAngles);

                    if (EditorGUI.EndChangeCheck())
                    {
                        startVal = Utils.QuaternionToVector4(Quaternion.Euler(startRot));
                        endVal = Utils.QuaternionToVector4(Quaternion.Euler(endRot));
                    }
                    break;
                case ValueType.Bool:
                    startVal[0] = EditorGUI.Toggle(pos, startValProp.displayName, startVal[0] >= 1.0f) ? 1.0f : 0.0f;
                    pos.y += height + spacing;

                    endVal[0] = EditorGUI.Toggle(pos, endValProp.displayName, endVal[0] >= 1.0f) ? 1.0f : 0.0f;
                    break;
            }
            pos.y += height + spacing;

            if (EditorGUI.EndChangeCheck())
            {
                startValProp.vector4Value = startVal;
                endValProp.vector4Value = endVal;
            }

            EditorGUI.PropertyField(pos, property.FindPropertyRelative("transitionTime"));
            pos.y += height + spacing;

            EditorGUI.PropertyField(pos, property.FindPropertyRelative("playWithPrevious"));
            pos.y += height + spacing;

            EditorGUI.PropertyField(pos, property.FindPropertyRelative("easingFunction"));
            pos.y += height + spacing;

            EditorGUI.PropertyField(pos, property.FindPropertyRelative("valueType"));
    
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0.0f;
        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(GetSetterEvent(property));
            height += EditorGUIUtility.singleLineHeight * 7.0f;
            height += EditorGUIUtility.standardVerticalSpacing * 7.0f;
        }
        else
        {
            height += EditorGUIUtility.singleLineHeight;
        }

        return height;
    }
}
