using UnityEngine;

namespace SceneLoading
{
    public class SceneLoadRelay : MonoBehaviour
    {
        [SerializeField] int[] scenes;

        public void Load()
        {
            SceneLoader.Current.Load(scenes);
        }
    }
}