using Recounter.Store.Security;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Recounter.Store.Editor
{
    public class EditorSaveBrowser : EditorWindow
    {
        StoreData viewedSave;

        bool showFileList = true;
        bool showProperties = true;
        bool showSaveCreator = true;

        string newSaveName;
        string newSavePlayerName;

        [MenuItem("Tools/Editor Save Browser")]
        static void ShowWindow() => GetWindow<EditorSaveBrowser>("Editor Save Browser").Show();

        static void ListInfo<TInfo>(string label, TInfo[] infoSet, Func<TInfo, object> valueRetriever)
            where TInfo : MemberInfo
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (infoSet.Length == 0)
            {
                EditorGUILayout.LabelField($"No {label.ToLower()}.");
                return;
            }

            var style = new GUIStyle(EditorStyles.label) { richText = true, stretchHeight = true };

            foreach (var info in infoSet)
            {
                var value = valueRetriever(info);
                var isString = value is string;

                var displayValue =
                    value == null
                    ? "<color=red>Null</color>"
                    : (isString ? $"\"{value}\"" : value.ToString());

                EditorGUILayout.LabelField(info.Name + ':', displayValue, style);
            }
        }

        bool VerifyFileName(out string modifiedName)
        {
            if (string.IsNullOrWhiteSpace(newSaveName))
            {
                EditorGUILayout.HelpBox(
                    "Cannot use empty or whitespace file name.",
                    MessageType.Error);

                modifiedName = null;
                return false;
            }

            if (StoreSerializer.ValidateFileName(newSaveName, out modifiedName)) return true;

            EditorGUILayout.HelpBox(
                $"Save name contains invalid file characters. It will be saved as '{modifiedName + StoreSerializer.SaveFileEnding}'",
                MessageType.Warning);
            return true;
        }

        void OnGUI()
        {
            if (!Directory.Exists(StoreSerializer.GetSaveDirectory()))
            {
                EditorGUILayout.HelpBox("Save folder does not exist.", MessageType.Info);

                if (GUILayout.Button("Create"))
                    Directory.CreateDirectory(StoreSerializer.GetSaveDirectory());

                return;
            }

            EditorStyles.label.wordWrap = true;
            showSaveCreator = EditorGUILayout.BeginFoldoutHeaderGroup(showSaveCreator, "Save Creator");
            
            if (showSaveCreator)
            {
                EditorGUILayout.BeginHorizontal();
                newSaveName = EditorGUILayout.TextField("File Name", newSaveName);
                EditorGUILayout.LabelField(StoreSerializer.SaveFileEnding, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();

                newSavePlayerName = EditorGUILayout.TextField("Player Name", newSavePlayerName);

                if (VerifyFileName(out var willSaveAs))
                {
                    if (GUILayout.Button(StoreSerializer.AlreadyExists(willSaveAs) ? "Format Save" : "Create New Save"))
                        StoreData.CreateWithFile(newSavePlayerName);
                }

                if (viewedSave != null && willSaveAs == viewedSave.baseFileName)
                    StoreSerializer.LoadStore(viewedSave.FullFileName, out viewedSave);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showFileList = EditorGUILayout.BeginFoldoutHeaderGroup(showFileList, "Save Files On Disk");
            if (showFileList)
            {
                var folderContents = StoreSerializer.AllSaveFiles();

                if (folderContents.Length == 0)
                    EditorGUILayout.LabelField("Save folder empty.");

                foreach (var filePath in folderContents)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(fileName);

                    var isFocused = viewedSave != null && viewedSave.baseFileName == fileName;

                    if (isFocused)
                        EditorGUILayout.LabelField("Focused", EditorStyles.boldLabel, GUILayout.Width(52));
                    else if (GUILayout.Button("Focus", GUILayout.ExpandWidth(false)))
                        StoreSerializer.LoadStore(fileName, out viewedSave);

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
                    Process.Start(StoreSerializer.GetSaveDirectory());
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (viewedSave != null)
            {
                showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties, "Properties");
                if (showProperties)
                {
                    ListAllInfo(viewedSave);

                    if (GUILayout.Button("Refresh"))
                        StoreSerializer.LoadStore(viewedSave.FullFileName, out viewedSave);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        static void ListAllInfo(StoreData data)
        {
            var style = new GUIStyle(EditorStyles.label) { richText = true, fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField(data.WasAltered()
                ? "<color=red>Alteration detected.</color>"
                : "<color=green>No alteration detected.</color>",
                style);

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            ListInfo("Properties", typeof(StoreData).GetProperties(flags),
                info => info.GetValue(data));

            ListInfo("Fields", typeof(StoreData).GetFields(flags),
                info => info.GetValue(data));
        }
    }
}