using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] LoadingScreen loadingScreen;

        public static SceneLoader Current { get; private set; }
        
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var instance = Instantiate(Resources.Load<GameObject>("SceneLoader"));
            DontDestroyOnLoad(instance);
            Current = instance.GetComponent<SceneLoader>();
        }

        public void Load(params Scene[] scenes) =>
            Load(Array.ConvertAll(scenes, s => s.buildIndex));

        public void Load(params int[] scenes) =>
            StartCoroutine(LoadScenes(scenes));

        IEnumerator LoadScenes(params int[] scenes)
        {
            // Wait for loading screen to show.
            yield return loadingScreen.WaitToShow();
            
            var operations = new AsyncOperation[scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                operations[i] =
                    SceneManager.LoadSceneAsync(scenes[i], i > 0 ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            loadingScreen.Activate(operations);
        }
    }
}