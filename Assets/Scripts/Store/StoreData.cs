using System;
using UnityEngine;

namespace Recounter.Store
{
    public class StoreData
    {
        public static StoreData Current { get; private set; }

        public readonly string fileName;
        public readonly string name;
        public readonly DateTime creationTime;

        public string ToJson() => JsonUtility.ToJson(this);

        public static StoreData FromJson(string json) => JsonUtility.FromJson<StoreData>(json);

        StoreData(string name, string fileName)
        {
            this.name = name;
            this.fileName = fileName;
            creationTime = DateTime.Now;
        }

        public static StoreData CreateWithFile(string name)
        {
            var fileName = StoreSerializer.ToValidFileName(name);

            if (StoreSerializer.AlreadyExists(fileName)) throw new InvalidOperationException($"\"{fileName}\" already exists.");

            var data = new StoreData(name, fileName);

            data.Save();

            return data;
        }

        public static void SetCurrentStore(StoreData data) => Current = data;

        public bool Save() => StoreSerializer.TrySaveStore(this);
    }
}
