using UnityEditor;
using UnityEngine;
using System.IO;
using System;

[CustomEditor(typeof(InteractableIconSettings))]
public class InteractableIconSettingsEditor : Editor
{
    static bool CheckIconsAgainstEnum(SerializedProperty arrayProp, string[] names)
    {
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

        return message == "";
    }

    static void TryPopulateIconAssets(string folder, SerializedProperty arrayProp, string[] names)
    {
        folder = folder[(Application.dataPath.Length - "Assets".Length)..];

        for (int i = 0; i < arrayProp.arraySize && i < names.Length; i++)
        {
            var existingReference = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue;
            if (existingReference != null && existingReference.name == names[i]) continue;

            var path = Path.Combine(folder, names[i] + ".png");

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (!sprite) continue;

            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
        }

        arrayProp.serializedObject.ApplyModifiedProperties();
    }

    static void FixArrayLength(SerializedProperty arrayProp, int length)
    {
        var difference = Mathf.Abs(length - arrayProp.arraySize);
        if (arrayProp.arraySize > length)
        {
            for (int i = 0; i < difference; i++)
            {
                arrayProp.DeleteArrayElementAtIndex(arrayProp.arraySize - 1);
            }
        }
        else if (arrayProp.arraySize < length)
        {
            for (int i = 0; i < difference; i++)
            {
                if (arrayProp.arraySize == 0)
                {
                    arrayProp.InsertArrayElementAtIndex(0);
                }

                arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize - 1);
            }
        }

        arrayProp.serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var interactIcons = serializedObject.FindProperty("icons");
        var interactNames = System.Enum.GetNames(typeof(Interactable.Icon));

        if (interactIcons.arraySize != interactNames.Length)
        {
            EditorGUILayout.HelpBox(
                $"{interactIcons.displayName} has the wrong length. You need {interactNames.Length}.",
                MessageType.Error);

            if (GUILayout.Button("Fix"))
            {
                FixArrayLength(interactIcons, interactNames.Length);
            }

            return;
        }

        var good = CheckIconsAgainstEnum(interactIcons, interactNames);

        if (good) return;

        if (GUILayout.Button("Fix Incorrect Assets"))
        {
            var path = AssetDatabase.GetAssetPath(target as ScriptableObject);
            var folder = EditorUtility.OpenFolderPanel("Load Icons", path, "");

            TryPopulateIconAssets(folder, interactIcons, interactNames);
        }
    }
}
