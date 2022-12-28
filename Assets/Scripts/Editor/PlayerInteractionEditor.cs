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
}
