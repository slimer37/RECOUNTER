using Newtonsoft.Json;
using System;

namespace Recounter.Store
{
    [Serializable]
    public class StoreData
    {
        public static StoreData Current { get; private set; }

        public string fileName;
        public string name;
        public DateTime creationTime;

        public string ToJson() => JsonConvert.SerializeObject(this);
        
        public static StoreData FromJson(string json) => JsonConvert.DeserializeObject<StoreData>(json);

        StoreData() { }

        StoreData(string name, string fileName)
        {
            this.name = name;
            this.fileName = fileName;
            creationTime = DateTime.Now;
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
