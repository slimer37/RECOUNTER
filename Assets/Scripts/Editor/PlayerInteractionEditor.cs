using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerInteraction))]
public class PlayerInteractionEditor : Editor
{
    [DrawGizmo(GizmoType.Active | GizmoType.Selected)]
    static void OnDrawGizmos(PlayerInteraction playerInteraction, GizmoType type)
    {
        var so = new SerializedObject(playerInteraction);
        var cam = so.FindProperty("cam").objectReferenceValue;

        if (!cam) return;
        Gizmos.color = Color.yellow;

        var range = so.FindProperty("range").floatValue;
        Gizmos.DrawWireSphere(((Transform)cam).position, range);
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
