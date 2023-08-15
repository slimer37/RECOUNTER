using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;

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

        public void Load(int mainIndex = 0, params AssetReference[] scenes) =>
            StartCoroutine(LoadScenes(mainIndex, scenes));

        IEnumerator LoadScenes(int mainIndex, params AssetReference[] scenes)
        {
            if (mainIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mainIndex), "Value cannot be negative.");
            }

            Time.timeScale = 1;

            // Wait for loading screen to show.
            yield return loadingScreen.WaitToShow();

            var operations = new AsyncOperationHandle[scenes.Length];

            for (var i = 0; i < scenes.Length; i++)
            {
                operations[i] =
                    scenes[i].LoadSceneAsync(i > 0 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            if (mainIndex > 0)
            {
                operations[mainIndex].Completed +=
                    r => SceneManager.SetActiveScene(((SceneInstance)r.Result).Scene);
            }

            loadingScreen.Activate(operations);
        }
    }
}