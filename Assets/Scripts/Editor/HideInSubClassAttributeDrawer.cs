using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideInSubClassAttribute))]
public class HideInSubClassAttributeDrawer : PropertyDrawer
{
    bool ShouldShow(SerializedProperty property)
    {
        var type = property.serializedObject.targetObject.GetType();
        var field = type.GetField(property.name);
        return field != null;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
            EditorGUI.PropertyField(position, property);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        ShouldShow(property) ? base.GetPropertyHeight(property, label) : 0;
}