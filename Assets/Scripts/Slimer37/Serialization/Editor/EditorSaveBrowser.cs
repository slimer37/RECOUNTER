using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Slimer37.Serialization.Editor
{
    public class EditorSaveBrowser : EditorWindow
    {
        SaveData _viewedSave;

        bool _showFileList = true;
        bool _showProperties = true;
        bool _showSaveCreator = true;

        string _newSaveName;
        string _newSavePlayerName;

        [MenuItem("Tools/Editor Save Browser")]
        static void ShowWindow() => GetWindow<EditorSaveBrowser>("Editor Save Browser").Show();

        static void ListInfo<TInfo>(string label, TInfo[] infoSet, Func<TInfo, object> valueRetriever,
            Func<TInfo, Type> typeRetriever)
            where TInfo : MemberInfo
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
                var value = valueRetriever(info);
                var isString = value is string;

                var type = typeRetriever(info).Name;

                var displayValue =
                    value == null
                    ? "<color=red>Null</color>"
                    : isString ? $"\"{value}\"" : value.ToString();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(type, style, GUILayout.Width(100));
                GUILayout.Label($"<b>{info.Name}</b>", style, GUILayout.Width(125));
                GUILayout.Label(displayValue, style);
                EditorGUILayout.EndHorizontal();
            }
        }

        bool VerifyFileName(out string modifiedName)
        {
            if (string.IsNullOrWhiteSpace(_newSaveName))
            {
                EditorGUILayout.HelpBox(
                    "Cannot use empty or whitespace file name.",
                    MessageType.Error);

                modifiedName = null;
                return false;
            }

            if (GameSerializer.ValidateFileName(_newSaveName, out modifiedName)) return true;

            EditorGUILayout.HelpBox(
                $"Save name contains invalid file characters. It will be saved as '{modifiedName + GameSerializer.SaveFileEnding}'",
                MessageType.Warning);
            return true;
        }

        void OnGUI()
        {
            if (!Directory.Exists(GameSerializer.GetSaveDirectory()))
            {
                EditorGUILayout.HelpBox("Save folder does not exist.", MessageType.Info);

                if (GUILayout.Button("Create"))
                    Directory.CreateDirectory(GameSerializer.GetSaveDirectory());

                return;
            }

            EditorStyles.label.wordWrap = true;
            _showSaveCreator = EditorGUILayout.BeginFoldoutHeaderGroup(_showSaveCreator, "Save Creator");

            if (_showSaveCreator)
            {
                EditorGUILayout.BeginHorizontal();
                _newSaveName = EditorGUILayout.TextField("File Name", _newSaveName);
                EditorGUILayout.LabelField(GameSerializer.SaveFileEnding, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();

                _newSavePlayerName = EditorGUILayout.TextField("Player Name", _newSavePlayerName);

                if (VerifyFileName(out var willSaveAs))
                {
                    if (GUILayout.Button(GameSerializer.AlreadyExists(willSaveAs) ? "Format Save" : "Create New Save"))
                        SaveData.CreateWithFile(_newSavePlayerName);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            _showFileList = EditorGUILayout.BeginFoldoutHeaderGroup(_showFileList, "Save Files On Disk");
            if (_showFileList)
            {
                var folderContents = GameSerializer.AllSaveFiles();

                if (folderContents.Length == 0)
                    EditorGUILayout.LabelField("Save folder empty.");

                foreach (var filePath in folderContents)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(fileName);

                    var isFocused = _viewedSave != null && _viewedSave.baseFileName == fileName;

                    if (isFocused)
                        EditorGUILayout.LabelField("Focused", EditorStyles.boldLabel, GUILayout.Width(52));
                    else if (GUILayout.Button("Focus", GUILayout.ExpandWidth(false)))
                        GameSerializer.LoadSave(fileName, out _viewedSave);

                    if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                    {
                        File.Delete(filePath);
                        if (isFocused)
                            _viewedSave = null;
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Open In Explorer"))
                    Process.Start(GameSerializer.GetSaveDirectory());
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (_viewedSave != null)
            {
                _showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(_showProperties, "Properties");
                if (_showProperties)
                {
                    ListAllInfo(_viewedSave);

                    if (GUILayout.Button("Refresh"))
                        GameSerializer.LoadSave(_viewedSave.FullFileName, out _viewedSave);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        // https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
        static void GUILine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        static void ListAllInfo(SaveData data)
        {
            var style = new GUIStyle(EditorStyles.label) { richText = true, fontStyle = FontStyle.Bold };

            EditorGUILayout.Space();

            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField(data.WasAltered()
                ? "<color=red>Alteration detected.</color>"
                : "<color=green>No alteration detected.</color>",
                style);

            EditorGUILayout.Space();

            style.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type", style, GUILayout.Width(100));
            GUILayout.Label("Name", style, GUILayout.Width(125));
            GUILayout.Label("Value", style);
            EditorGUILayout.EndHorizontal();

            GUILine();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            ListInfo("Properties", typeof(SaveData).GetProperties(flags),
                info => info.GetValue(data),
                info => info.PropertyType);

            ListInfo("Fields", typeof(SaveData).GetFields(flags)
                .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false))
                .ToArray(),
                info => info.GetValue(data),
                info => info.FieldType);

            EditorGUILayout.Space();
        }
    }
}