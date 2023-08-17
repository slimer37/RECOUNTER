using Recounter.Store;
using SceneLoading;
using UnityEngine;

namespace Recounter
{
    public class GameManager : MonoBehaviour
    {
        public static StoreData StoreData { get; private set; }

        public static GameManager Singleton { get; private set; }

        const string StoreSceneKey = "Store";

        void Awake()
        {
            if (Singleton)
                throw new System.Exception("A GameManager instance already exists!");

            Singleton = this;
        }

        void OnDestroy()
        {
            Singleton = null;
        }

        static void LoadGameScene() => SceneLoader.Current.Load(true, StoreSceneKey);

        public static void StartGame(StoreData source)
        {
            StoreData = source;

            LoadGameScene();
        }
    }
}
