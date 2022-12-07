using UnityEngine;
using System.IO;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

public static class SaveFileManager
{
    public const string SaveFolderName = "saves";
    public const string SaveFileEnding = ".store";

    public static readonly string SaveFolder =
        Path.Combine(Application.persistentDataPath, SaveFolderName);

    public static readonly string InvalidCharacters =
            new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

    public static bool NameIsValid(string fileName) =>
        !string.IsNullOrWhiteSpace(fileName)
        && !fileName.Any(c => InvalidCharacters.Contains(c));

    public static string ToValidFileName(string fileName) =>
        InvalidCharacters.Aggregate(fileName, (current, c) => current.Replace(c.ToString(), "-"));

    public static bool FileExists(string fileName) =>
        GetSaveFiles().Contains(Path.Combine(SaveFolder, fileName));

    [RuntimeInitializeOnLoadMethod]
    static void InitFolder()
    {
        if (Directory.Exists(SaveFolder)) return;

        Directory.CreateDirectory(SaveFolder);
    }

    public static string[] GetSaveFiles() =>
        Directory.GetFiles(SaveFolder, "*" + SaveFileEnding);

    public static void Save(GameData data, string fileName)
    {
        data.Guard();

        var path = Path.Combine(SaveFolder, fileName + SaveFileEnding);

        var contents = JsonConvert.SerializeObject(data);
        var bytes = Encoding.UTF8.GetBytes(contents);
        var base64 = Convert.ToBase64String(bytes);
        File.WriteAllText(path, base64);
    }

    public static GameData Load(string fileName)
    {
        var path = Path.Combine(SaveFolder, fileName + SaveFileEnding);

        try
        {
            var contents = File.ReadAllText(path);
            var base64 = Convert.FromBase64String(contents);
            var json = Encoding.UTF8.GetString(base64);

            var retrievedData = new GameData() { AccessPath = path };
            JsonConvert.PopulateObject(json, retrievedData);

            return retrievedData;
        }
        catch (Exception error)
        {
            Debug.LogError($"Exception loading at {path}:\n{error}");
            return null;
        }
    }
}
