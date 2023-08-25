using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Slimer37.SceneLoading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] LoadingScreen loadingScreen;

        const string BaseSceneKey = "Base Scene";

        public static SceneLoader Current { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var prefab = Resources.Load<GameObject>("SceneLoader");

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
                var key = sceneKeys[i];
                var mode = i > 0 ? LoadSceneMode.Additive : LoadSceneMode.Single;

                operations[i] = Addressables.LoadSceneAsync(key, mode);
            }

            loadingScreen.Activate(operations);

            if (!withBase) yield break;

            // Wait for base scene and primary scene to be loaded in.
            yield return new WaitUntil(() => operations[0].IsDone && operations[1].IsDone);

            var primaryScene = ((SceneInstance)operations[1].Result).Scene;

            SceneManager.SetActiveScene(primaryScene);
        }
    }
}