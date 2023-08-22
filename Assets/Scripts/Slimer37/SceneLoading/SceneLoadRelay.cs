using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Slimer37.SceneLoading
{
    public class SceneLoadRelay : MonoBehaviour
    {
        [SerializeField] bool withBase;
        [SerializeField] AssetReference[] scenes;

        public void Load()
        {
            SceneLoader.Current.Load(withBase, scenes);
        }
    }
}