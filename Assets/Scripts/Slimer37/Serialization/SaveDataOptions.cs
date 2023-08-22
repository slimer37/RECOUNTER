using UnityEngine;

namespace Slimer37.Serialization
{
    [CreateAssetMenu(fileName = "SaveDataOptions", menuName = "Settings/Save Data Options")]
    internal class SaveDataOptions : ScriptableObject
    {
        [field: SerializeField] public string SaveFolderName { get; private set; } = "saves";
        [field: SerializeField] public string SaveFileEnding { get; private set; } = ".save";
        [field: SerializeField] public bool EnableDebugMessages { get; private set; }
    }
}
