using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Recounter.SceneLoading.Editor
{
    public static class BaseSceneLoader
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.name != "Demo" && scene.name != "Store") return;

            EditorSceneManager.OpenScene("Assets/Scenes/Base.unity", OpenSceneMode.Additive);
        }
    }
}
