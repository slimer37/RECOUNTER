using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorSaveBrowser : EditorWindow
{
    GameData viewedSave;
    bool showFileList = true;
    bool showProperties = true;
    bool showSaveCreator = true;

    string newSaveName;
    string newSavePlayerName;

    [MenuItem("Tools/Editor Save Browser")]
    static void ShowWindow() => GetWindow<EditorSaveBrowser>("Editor Save Browser").Show();

    static void ListInfo<TInfo>(string label, TInfo[] infoSet, Func<TInfo, object> valueRetriever,
        Func<TInfo, bool> itemConditionChecker = null) where TInfo : MemberInfo
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (infoSet.Length == 0)
        {
            EditorGUILayout.LabelField($"No {label.ToLower()}.");
            return;
        }

        var style = new GUIStyle(EditorStyles.label) { richText = true };

        foreach (var info in infoSet)
        {
            if (!itemConditionChecker?.Invoke(info) ?? false) continue;

            var value = valueRetriever(info);
            var isString = value is string;

            EditorGUILayout.LabelField(info.Name + ':',
                value == null ? "<color=red>Null</color>" :
                    (isString ? $"\"{value}\"" : value.ToString()),
                style);
        }
    }

    bool VerifyFileName(out string modifiedName)
    {
        modifiedName = newSaveName;
        if (SaveFileManager.NameIsValid(newSaveName)) return true;
        if (string.IsNullOrWhiteSpace(newSaveName))
        {
            EditorGUILayout.HelpBox(
                "Cannot use empty or whitespace file name.",
                MessageType.Error);
            return false;
        }

        modifiedName = SaveFileManager.ToValidFileName(newSaveName);
        EditorGUILayout.HelpBox(
            $"Save name contains invalid file characters. It will be saved as '{modifiedName + SaveFileManager.SaveFileEnding}'",
            MessageType.Warning);
        return true;
    }

    void OnGUI()
    {
        if (!Directory.Exists(SaveFileManager.SaveFolder))
        {
            EditorGUILayout.HelpBox("Save folder does not exist.", MessageType.Info);

            if (GUILayout.Button("Create"))
                Directory.CreateDirectory(SaveFileManager.SaveFolder);

            return;
        }

        EditorStyles.label.wordWrap = true;
        showSaveCreator = EditorGUILayout.BeginFoldoutHeaderGroup(showSaveCreator, "Save Creator");
        if (showSaveCreator)
        {
            EditorGUILayout.BeginHorizontal();
            newSaveName = EditorGUILayout.TextField("File Name", newSaveName);
            EditorGUILayout.LabelField(SaveFileManager.SaveFileEnding, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            newSavePlayerName = EditorGUILayout.TextField("Player Name", newSavePlayerName);

            if (VerifyFileName(out var willSaveAs))
            {
                if (GUILayout.Button(SaveFileManager.FileExists(willSaveAs) ? "Format Save" : "Create New Save"))
                    SaveFileManager.Save(new GameData(willSaveAs, newSavePlayerName), willSaveAs);
            }

            if (viewedSave != null && willSaveAs == Path.GetFileNameWithoutExtension(viewedSave.AccessPath))
                viewedSave = SaveFileManager.Load(Path.GetFileNameWithoutExtension(viewedSave.AccessPath));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        showFileList = EditorGUILayout.BeginFoldoutHeaderGroup(showFileList, "Save Files On Disk");
        if (showFileList)
        {
            var folderContents = SaveFileManager.GetSaveFiles();

            if (folderContents.Length == 0)
                EditorGUILayout.LabelField("Save folder empty.");

            foreach (var filePath in folderContents)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(fileName);

                var isFocused = viewedSave != null && Path.GetFileNameWithoutExtension(viewedSave.AccessPath) == fileName;

                if (isFocused)
                    EditorGUILayout.LabelField("Focused", EditorStyles.boldLabel, GUILayout.Width(52));
                else if (GUILayout.Button("Focus", GUILayout.ExpandWidth(false)))
                    viewedSave = SaveFileManager.Load(fileName);

                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                {
                    File.Delete(filePath);
                    if (isFocused)
                        viewedSave = null;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Open In Explorer"))
                Process.Start(SaveFileManager.SaveFolder);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (viewedSave != null)
        {
            showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties, "Properties");
            if (showProperties)
            {
                ListAllInfo(viewedSave);

                if (GUILayout.Button("Refresh"))
                    viewedSave = SaveFileManager.Load(viewedSave.AccessPath);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    static void ListAllInfo(GameData data)
    {
        var style = new GUIStyle(EditorStyles.label) { richText = true, fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField(data.WasAltered()
            ? "<color=red>Alteration detected.</color>"
            : "<color=green>No alteration detected.</color>",
            style);

        ListInfo("Properties", typeof(GameData).GetProperties(),
            info => info.GetValue(data),
            info => !info.GetMethod.IsStatic);

        ListInfo("Fields", typeof(GameData).GetFields(),
            info => info.GetValue(data),
            info => !info.IsStatic);
    }
}