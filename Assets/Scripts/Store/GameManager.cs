using Recounter.Store;
using SceneLoading;
using UnityEngine;

namespace Recounter
{
    public class GameManager : MonoBehaviour
    {
        public static StoreData StoreData
        {
            get
            {
                if (s_storeData == null)
                {
                    Debug.LogWarning("No store data is loaded. Loading temporary data.");
                    s_storeData = StoreData.CreateTemporary();
                }

                return s_storeData;
            }
        }

        public static GameManager Singleton { get; private set; }

        static StoreData s_storeData;

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
            s_storeData = source;

            LoadGameScene();
        }
    }
}
