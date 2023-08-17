using Newtonsoft.Json;
using Recounter.Store.Security;
using System;
using System.IO;

namespace Recounter.Store
{
    [Serializable, JsonObject(memberSerialization: MemberSerialization.OptIn)]
    public class StoreData
    {
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

        public static StoreData CreateTemporary() => new("Temporary", "temporary save");

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
            protection = SaveGuard.GetShaHash(this);
            StoreSerializer.SaveStore(this);
        }

        public void Delete()
        {
            File.Delete(StoreSerializer.GetSavePath(baseFileName));
            FileExists = false;
            Deleted?.Invoke();
        }
    }
}
