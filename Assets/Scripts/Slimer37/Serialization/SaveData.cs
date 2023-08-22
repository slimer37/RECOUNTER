using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Slimer37.Serialization
{
    [Serializable, JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class SaveData
    {
        [JsonProperty] public string name;
        [JsonProperty] public readonly DateTime creationTime;
        [JsonProperty] public string protection;

        [JsonProperty] readonly Dictionary<string, object> _data = new();

        public readonly string baseFileName;

        public bool FileExists { get; private set; } = true;

        public event Action Deleted;
        public event Action Saved;
        public event Action PreSave;

        public string FullFileName => baseFileName + GameSerializer.SaveFileEnding;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        static SaveData s_temporaryData;

        public bool IsTemporary => this == s_temporaryData;

        public static SaveData Temporary
        {
            get
            {
                if (s_temporaryData == null)
                {
                    s_temporaryData = new("Temporary", "TEMPORARY SAVE");
                    UnityEngine.Debug.Log($"Created temporary {nameof(SaveData)}.");
                }

                return s_temporaryData;
            }
        }

        public void SetKey<T>(string key, T value)
        {
            _data[key] = value;

            UnityEngine.Debug.Log(_data[key]);
        }

        public bool TryGetKey<T>(string key, out T value)
        {
            if (_data.ContainsKey(key))
            {
                // Hack to force object deserialization by property name.
                value = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(_data[key]));
                return true;
            }

            value = default;
            return false;
        }

        internal static SaveData FromJson(string json, string accessPath)
        {
            var data = new SaveData(accessPath);
            JsonConvert.PopulateObject(json, data);
            return data;
        }

        SaveData(string name, string accessPath) : this(accessPath)
        {
            this.name = name;
            creationTime = DateTime.Now;
        }

        SaveData(string accessPath)
        {
            baseFileName = Path.GetFileNameWithoutExtension(accessPath);
        }

        public static SaveData CreateWithFile(string name)
        {
            GameSerializer.ValidateFileName(name, out var fileName);

            if (GameSerializer.AlreadyExists(fileName)) throw new InvalidOperationException($"\"{fileName}\" already exists.");

            var data = new SaveData(name, fileName);

            data.Save();

            return data;
        }

        public void Save()
        {
            if (IsTemporary)
            {
                UnityEngine.Debug.LogWarning($"Will not save temporary {nameof(SaveData)}.");
                return;
            }

            PreSave?.Invoke();

            protection = SaveGuard.GetShaHash(this);
            GameSerializer.WriteSave(this);

            UnityEngine.Debug.Log($"Just saved \"{name}\"");

            Saved?.Invoke();
        }

        public void Delete()
        {
            File.Delete(GameSerializer.GetSavePath(baseFileName));
            FileExists = false;
            Deleted?.Invoke();
        }
    }
}
