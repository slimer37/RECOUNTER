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
        var camTransform = (Transform)cam;

        var arrowSize = Mathf.Min(0.5f, range);

        var start = camTransform.position;
        var dir = camTransform.forward;
        var end = start + dir * (range - arrowSize);

        Handles.DrawDottedLine(start, end, 5);

        Handles.ArrowHandleCap(0, end, camTransform.rotation, arrowSize, EventType.Repaint);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var icons = serializedObject.FindProperty("icons");
        var names = System.Enum.GetNames(typeof(Interactable.Icon));

        if (icons.arraySize != names.Length)
        {
            EditorGUILayout.HelpBox($"Wrong number of icons. You need {names.Length}.", MessageType.Error);
        }

        var message = "";

        for (int i = 0; i < icons.arraySize && i < names.Length; i++)
        {
            var iconName = icons.GetArrayElementAtIndex(i).objectReferenceValue?.name ?? "null";
            if (iconName != names[i])
            {
                message += $"Icon {i} (\"{iconName}\") does not match enum name \"{names[i]}\"\n";
            }
        }

        if (message != "")
        {
            EditorGUILayout.HelpBox(message[..^1], MessageType.Warning);
        }
    }
}
