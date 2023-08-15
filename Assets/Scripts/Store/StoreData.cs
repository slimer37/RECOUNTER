using Newtonsoft.Json;
using Recounter.Store.Security;
using System;
using System.IO;

namespace Recounter.Store
{
    [Serializable, JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class StoreData
    {
        public static StoreData Current { get; private set; }

        [JsonProperty] public string name;
        [JsonProperty] public readonly DateTime creationTime;
        [JsonProperty] public string protection;

        public readonly string baseFileName;

        public bool FileExists { get; private set; } = true;

        public event Action Deleted;

        public string FullFileName => baseFileName + StoreSerializer.SaveFileEnding;

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static StoreData FromJson(string json, string accessPath)
        {
            var data = new StoreData(accessPath);
            JsonConvert.PopulateObject(json, data);
            return data;
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

        public static void SetCurrentStore(StoreData data) => Current = data;

        public bool Save()
        {
            protection = SaveGuard.GetShaHash(this);
            return StoreSerializer.TrySaveStore(this);
        }

        public void Delete()
        {
            File.Delete(StoreSerializer.GetSavePath(baseFileName));
            FileExists = false;
            Deleted?.Invoke();
        }
    }
}
