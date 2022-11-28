using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SceneLoading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] LoadingScreen loadingScreen;

        public static SceneLoader Current { get; private set; }
        
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>("SceneLoader").WaitForCompletion();
            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
            Current = instance.GetComponent<SceneLoader>();
        }

        public void Load(params AssetReference[] scenes) =>
            StartCoroutine(LoadScenes(scenes));

        IEnumerator LoadScenes(params AssetReference[] scenes)
        {
            // Wait for loading screen to show.
            yield return loadingScreen.WaitToShow();
            
            var operations = new AsyncOperationHandle[scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                operations[i] =
                    scenes[i].LoadSceneAsync(i > 0 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            loadingScreen.Activate(operations);
        }
    }
}