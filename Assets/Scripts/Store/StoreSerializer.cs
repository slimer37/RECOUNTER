using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Recounter.StoreData
{
    public static class StoreSerializer
    {
        const string SaveFolderName = "saves";
        const string SaveFileEnding = ".store";

        public static string ToValidFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
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

        public static string GetSavePath(string saveFileName) => Path.Combine(GetSaveDirectory(), saveFileName + SaveFileEnding);

        public static bool TryLoadSavedStore(string saveFileName, out StoreData storeData)
        {
            storeData = null;

            var path = GetSavePath(saveFileName);

            if (!File.Exists(path)) return false;

            storeData = StoreData.FromJson(File.ReadAllText(path));

            return true;
        }

        public static bool TrySaveStore(StoreData storeData)
        {
            try
            {
                File.WriteAllText(GetSavePath(storeData.fileName), storeData.ToJson());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while saving file: {e}");
                return false;
            }
        }

        public static string[] AllSaveFiles() => Directory.GetFiles(GetSaveDirectory(), $"*{SaveFileEnding}");
    }
}
