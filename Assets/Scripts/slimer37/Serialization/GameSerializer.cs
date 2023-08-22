using System;
using System.IO;
using UnityEngine;

namespace Slimer37.Serialization
{
    public static class GameSerializer
    {
        static readonly SaveDataOptions s_saveDataOptions;

        public static string SaveFileEnding => s_saveDataOptions.SaveFileEnding;
        public static string SaveFolderName => s_saveDataOptions.SaveFolderName;
        public static bool DebugEnabled => s_saveDataOptions.EnableDebugMessages;

        const string ResourcesSaveDataOptionsPath = "SaveDataOptions";

        static void PrintDebug(string text)
        {
            if (!DebugEnabled) return;

            Debug.Log(text);
        }

        static GameSerializer()
        {
            s_saveDataOptions = Resources.Load<SaveDataOptions>(ResourcesSaveDataOptionsPath);

            if (!s_saveDataOptions)
            {
                throw new FileNotFoundException($"Resources path \"{ResourcesSaveDataOptionsPath}\" was not found.");
            }

            PrintDebug("Initialized save system.");
        }

        public static string[] AllSaveFiles() => Directory.GetFiles(GetSaveDirectory(), $"*{SaveFileEnding}");

        public static bool ValidateFileName(string fileName, out string validFileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null or whitespace.");
            }

            validFileName = fileName;

            // Replace ending ".store" with "_store"
            if (fileName.EndsWith(SaveFileEnding))
            {
                var index = fileName.Length - SaveFileEnding.Length;
                validFileName = validFileName.Remove(index, 1);
                validFileName = validFileName.Insert(index, "_");
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                validFileName = validFileName.Replace(c, '_');
            }

            return validFileName == fileName;
        }

        public static string GetSaveDirectory()
        {
            var saveDirectory = Path.Combine(Application.persistentDataPath, SaveFolderName);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            return saveDirectory;
        }

        public static bool AlreadyExists(string querySaveFile) => File.Exists(GetSavePath(querySaveFile));

        public static void LoadSave(string saveFilePath, out SaveData storeData)
        {
            storeData = null;

            if (!Path.IsPathRooted(saveFilePath))
            {
                saveFilePath = GetSavePath(saveFilePath);
            }

            if (!File.Exists(saveFilePath))
                throw new FileNotFoundException($"Save file \"{saveFilePath}\" was not found.");

            storeData = SaveData.FromJson(File.ReadAllText(saveFilePath), saveFilePath);
        }

        internal static void WriteSave(SaveData storeData)
        {
            try
            {
                File.WriteAllText(GetSavePath(storeData.baseFileName), storeData.ToJson());
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while saving file", e);
            }
        }

        internal static string GetSavePath(string saveFileName)
        {
            if (!saveFileName.EndsWith(SaveFileEnding))
                saveFileName += SaveFileEnding;

            return Path.Combine(GetSaveDirectory(), saveFileName);
        }
    }
}
