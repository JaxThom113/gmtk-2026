using UnityEngine;
using UnityEditor;

namespace KevinCastejon.MissingFeatures.MissingAttributes
{
    [CustomPropertyDrawer(typeof(ShowPropIfAttribute))]
    public class ShowPropIfDrawer : PropertyDrawer
    {
        /*public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowPropIfAttribute att = (ShowPropIfAttribute)attribute;
            if (property.serializedObject.FindProperty(att.boolSerializedPropertyName).boolValue == att.isTrue)
            {
                return EditorGUI.GetPropertyHeight(property, label, false);
            }
            else return 0f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowPropIfAttribute att = (ShowPropIfAttribute)attribute;
            if (property.serializedObject.FindProperty(att.boolSerializedPropertyName).boolValue == att.isTrue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }*/
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var att = (ShowPropIfAttribute)attribute;

            // 1. Safely find the condition property
            SerializedProperty conditionProp = property.serializedObject.FindProperty(att.boolSerializedPropertyName);

            // If the condition prop doesn't exist, default to showing it (fail-safe)
            if (conditionProp == null) return EditorGUI.GetPropertyHeight(property, label, true);

            // 2. Check the condition
            if (conditionProp.propertyType == SerializedPropertyType.Boolean &&
                conditionProp.boolValue == att.isTrue)
            {
                // Return the actual height of the array (including its header/foldout)
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            // Condition failed: Return 0 to hide the entire thing
            return 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (ShowPropIfAttribute)attribute;

            SerializedProperty conditionProp = property.serializedObject.FindProperty(att.boolSerializedPropertyName);

            if (conditionProp == null || conditionProp.propertyType != SerializedPropertyType.Boolean)
            {
                // Fallback: Draw normally if condition is broken
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            if (conditionProp.boolValue == att.isTrue)
            {
                // Draw the array property normally
                // Unity will automatically draw the foldout "[0] [1]..." and all contents here
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                // Do nothing. 
                // Because GetPropertyHeight returned 0, this GUI call is effectively ignored
                // and no space is reserved in the layout.
            }
        }
    }
}