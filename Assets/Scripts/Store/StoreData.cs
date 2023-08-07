using Newtonsoft.Json;
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

        public readonly string fileName;

        public string ToJson() => JsonConvert.SerializeObject(this);

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
            fileName = Path.GetFileName(accessPath);
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

        public bool Save() => StoreSerializer.TrySaveStore(this);
    }
}
