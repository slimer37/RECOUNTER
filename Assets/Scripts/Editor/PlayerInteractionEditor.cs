using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerInteraction))]
public class PlayerInteractionEditor : Editor
{
    void OnSceneGUI()
    {
        var cam = serializedObject.FindProperty("cam").objectReferenceValue;

        if (!cam) return;

        Handles.color = Color.cyan;

        var range = serializedObject.FindProperty("range").floatValue;
        var camTransform = ((Camera)cam).transform;

        var arrowSize = Mathf.Min(0.5f, range);

        var start = camTransform.position;
        var dir = camTransform.forward;
        var end = start + dir * (range - arrowSize);

        Handles.DrawDottedLine(start, end, 5);

        Handles.ArrowHandleCap(0, end, camTransform.rotation, arrowSize, EventType.Repaint);
    }

    static void CheckIconsAgainstEnum(SerializedProperty arrayProp, string[] names)
    {
        if (arrayProp.arraySize != names.Length)
            EditorGUILayout.HelpBox($"{arrayProp.displayName} has the wrong length. You need {names.Length}.", MessageType.Error);

        var message = "";

        for (int i = 0; i < arrayProp.arraySize && i < names.Length; i++)
        {
            var element = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue;
            var objName = element ? element.name : "null";
            if (objName != names[i])
            {
                message += $"Icon {i} (\"{objName}\") does not match enum name \"{names[i]}\"\n";
            }
        }

        if (message != "")
            EditorGUILayout.HelpBox($"{arrayProp.displayName} incorrect:\n" + message[..^1], MessageType.Warning);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var interactIcons = serializedObject.FindProperty("icons");
        var interactNames = System.Enum.GetNames(typeof(Interactable.Icon));

        CheckIconsAgainstEnum(interactIcons, interactNames);
    }
}
