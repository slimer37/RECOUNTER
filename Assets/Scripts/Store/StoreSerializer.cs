using System;
using System.IO;
using UnityEngine;

namespace Recounter.Store
{
    public static class StoreSerializer
    {
        public const string SaveFolderName = "saves";
        public const string SaveFileEnding = ".store";

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

        public static string GetSavePath(string saveFileName)
        {
            if (!saveFileName.EndsWith(SaveFileEnding))
                saveFileName += SaveFileEnding;

            return Path.Combine(GetSaveDirectory(), saveFileName);
        }

        public static void LoadStore(string saveFilePath, out StoreData storeData)
        {
            storeData = null;

            if (!Path.IsPathRooted(saveFilePath))
            {
                saveFilePath = GetSavePath(saveFilePath);
            }

            if (!File.Exists(saveFilePath))
                throw new FileNotFoundException($"Save file \"{saveFilePath}\" was not found.");

            storeData = StoreData.FromJson(File.ReadAllText(saveFilePath), saveFilePath);
        }

        public static void SaveStore(StoreData storeData)
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

        public static string[] AllSaveFiles() => Directory.GetFiles(GetSaveDirectory(), $"*{SaveFileEnding}");
    }
}
