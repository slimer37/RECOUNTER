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

        public static bool TryLoadSavedStore(string saveFilePath, out StoreData storeData)
        {
            storeData = null;

            if (!Path.IsPathRooted(saveFilePath))
            {
                saveFilePath = GetSavePath(saveFilePath);
            }

            if (!File.Exists(saveFilePath)) return false;

            storeData = StoreData.FromJson(File.ReadAllText(saveFilePath), saveFilePath);

            return true;
        }

        public static bool TrySaveStore(StoreData storeData)
        {
            try
            {
                File.WriteAllText(GetSavePath(storeData.baseFileName), storeData.ToJson());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while saving file: {e}");
                return false;
            }
        }

        public static string[] AllSaveFiles() => Directory.GetFiles(GetSaveDirectory(), $"*{SaveFileEnding}");

        public static void Delete(StoreData storeData) => File.Delete(GetSavePath(storeData.baseFileName));
    }
}
