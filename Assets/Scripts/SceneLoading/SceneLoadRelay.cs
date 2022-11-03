using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SceneLoading
{
    public class SceneLoadRelay : MonoBehaviour
    {
        [SerializeField] AssetReference[] scenes;

        public void Load()
        {
            SceneLoader.Current.Load(scenes);
        }
    }
}