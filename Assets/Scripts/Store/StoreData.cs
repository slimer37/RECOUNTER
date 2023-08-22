using Newtonsoft.Json;
using Recounter.Store.Security;
using System;
using System.Collections.Generic;
using System.IO;

namespace Recounter.Store
{
    [Serializable, JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class StoreData
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

        public string FullFileName => baseFileName + StoreSerializer.SaveFileEnding;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented,
            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

        static StoreData s_temporaryData;

        public bool IsTemporary => this == s_temporaryData;

        public static StoreData Temporary
        {
            get
            {
                if (s_temporaryData == null)
                {
                    s_temporaryData = new("Temporary", "TEMPORARY SAVE");
                    UnityEngine.Debug.Log($"Created temporary {nameof(StoreData)}.");
                }

                return s_temporaryData;
            }
        }

        public static StoreData FromJson(string json, string accessPath)
        {
            var data = new StoreData(accessPath);
            JsonConvert.PopulateObject(json, data);
            return data;
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

        StoreData(string name, string accessPath) : this(accessPath)
        {
            this.name = name;
            creationTime = DateTime.Now;
        }

        StoreData(string accessPath)
        {
            baseFileName = Path.GetFileNameWithoutExtension(accessPath);
        }

        public static StoreData CreateWithFile(string name)
        {
            StoreSerializer.ValidateFileName(name, out var fileName);

            if (StoreSerializer.AlreadyExists(fileName)) throw new InvalidOperationException($"\"{fileName}\" already exists.");

            var data = new StoreData(name, fileName);

            data.Save();

            return data;
        }

        public void Save()
        {
            if (IsTemporary)
            {
                UnityEngine.Debug.LogWarning($"Will not save temporary {nameof(StoreData)}.");
                return;
            }

            PreSave?.Invoke();

            protection = SaveGuard.GetShaHash(this);
            StoreSerializer.SaveStore(this);

            UnityEngine.Debug.Log($"Just saved \"{name}\"");

            Saved?.Invoke();
        }

        public void Delete()
        {
            File.Delete(StoreSerializer.GetSavePath(baseFileName));
            FileExists = false;
            Deleted?.Invoke();
        }
    }
}
