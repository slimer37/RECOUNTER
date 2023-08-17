using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace SceneLoading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] LoadingScreen loadingScreen;

        const string BaseSceneKey = "Base Scene";

        public static SceneLoader Current { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>("SceneLoader").WaitForCompletion();
            var instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
            Current = instance.GetComponent<SceneLoader>();
        }

        public void Load(bool withBase, params object[] sceneKeys) =>
            StartCoroutine(LoadScenes(withBase, sceneKeys));

        IEnumerator LoadScenes(bool withBase, params object[] sceneKeys)
        {
            var mainIndex = withBase ? 1 : 0;

            Time.timeScale = 1;

            // Wait for loading screen to show.
            yield return loadingScreen.WaitToShow();

            if (withBase)
            {
                var copy = new object[sceneKeys.Length + 1];

                copy[0] = BaseSceneKey;

                sceneKeys.CopyTo(copy, 1);

                sceneKeys = copy;
            }

            var operations = new AsyncOperationHandle[sceneKeys.Length];

            for (var i = 0; i < sceneKeys.Length; i++)
            {
                object key = sceneKeys[i];
                var mode = i > 0 ? LoadSceneMode.Additive : LoadSceneMode.Single;

                operations[i] = Addressables.LoadSceneAsync(key, mode);
            }

            if (withBase)
            {
                operations[1].Completed +=
                    r => SceneManager.SetActiveScene(((SceneInstance)r.Result).Scene);
            }

            loadingScreen.Activate(operations);
        }
    }
}