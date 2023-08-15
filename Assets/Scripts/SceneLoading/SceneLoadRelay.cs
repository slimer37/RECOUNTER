using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SceneLoading
{
    public class SceneLoadRelay : MonoBehaviour
    {
        [SerializeField] int mainIndex;
        [SerializeField] AssetReference[] scenes;

        public void Load()
        {
            SceneLoader.Current.Load(mainIndex, scenes);
        }
    }
}