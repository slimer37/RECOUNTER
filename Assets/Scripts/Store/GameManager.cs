using SceneLoading;
using slimer37.Serialization;
using UnityEngine;

namespace Recounter
{
    public class GameManager : MonoBehaviour
    {
        public static SaveData StoreData
        {
            get
            {
                if (s_storeData == null)
                {
                    Debug.LogWarning("No store data is loaded. Loading temporary data.");
                    s_storeData = SaveData.Temporary;
                }

                return s_storeData;
            }
        }

        public static GameManager Singleton { get; private set; }

        static SaveData s_storeData;

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

        public static void StartGame(SaveData source)
        {
            s_storeData = source;

            LoadGameScene();
        }
    }
}
