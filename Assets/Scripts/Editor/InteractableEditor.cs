using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable), true)]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var go = (target as Interactable).gameObject;
        var interactableLayer = LayerMask.NameToLayer("Interactable");

        if (go.layer != interactableLayer)
        {
            EditorGUILayout.HelpBox("Object has the wrong layer.", MessageType.Warning);

            if (GUILayout.Button("Fix", GUILayout.Height(30)))
            {
                Undo.RecordObject(go, "Fix layer on interactable");
                go.layer = interactableLayer;
            }
        }

        base.OnInspectorGUI();
    }
}
