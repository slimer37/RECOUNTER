using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Store
{
    public class SaveButton : MonoBehaviour
    {
        void Awake()
        {
            var button = GetComponent<Button>();

            if (GameManager.StoreData is null or { IsTemporary: true })
            {
                button.interactable = false;
                return;
            }

            button.onClick.AddListener(Save);
        }

        void Save()
        {
            GameManager.StoreData.Save();
        }
    }
}
