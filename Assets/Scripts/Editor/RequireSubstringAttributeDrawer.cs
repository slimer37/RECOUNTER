using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RequireSubstringAttribute))]
public class RequireSubstringAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        var substringAttribute = (RequireSubstringAttribute)attribute;

        var temp = position.yMax;
        if (substringAttribute.textArea)
        {
            var labelPos = EditorGUI.IndentedRect(position);
            labelPos.height = 18;
            position.yMin += labelPos.height;
            position.yMax = position.yMin + EditorStyles.textArea.lineHeight * substringAttribute.lines;
            EditorGUI.HandlePrefixLabel(position, labelPos, label);
            property.stringValue = EditorGUI.TextArea(position, property.stringValue, EditorStyles.textArea);
        }
        else
        {
            position.yMax = position.yMin + 18;
            property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
        }
        position.yMax = temp;

        var substrings = substringAttribute.substrings;
        if (!substrings.All(s => property.stringValue.Contains(s)))
        {
            position.yMin += substringAttribute.textArea ? EditorStyles.textArea.lineHeight * substringAttribute.lines : 18;
            position.yMin += 2;

            var substringList = "";
            foreach (var s in substrings)
                substringList += $", \"{s}\"";

            EditorGUI.HelpBox(position,
                $"{property.displayName} must contain each of the following substrings: {substringList.Substring(2)}",
                MessageType.Error);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var substringAttribute = (RequireSubstringAttribute)attribute;

        var height = substringAttribute.textArea
            ? 18 + EditorStyles.textArea.lineHeight * substringAttribute.lines
            : base.GetPropertyHeight(property, label);

        var substrings = substringAttribute.substrings;
        if (!substrings.All(s => property.stringValue.Contains(s)))
            height += EditorStyles.helpBox.lineHeight * 3;

        return height;
    }
}