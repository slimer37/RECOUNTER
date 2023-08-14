using Recounter.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.UI
{
    public class SaveInfoPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] Button _enter;
        [SerializeField] Button _delete;
        [SerializeField] CanvasGroup _group;

        StoreData _focusedSave;

        public void ResetFocus()
        {
            _group.interactable = false;
            _focusedSave = null;
        }

        void Awake()
        {
            _enter.onClick.AddListener(EnterStore);
            _delete.onClick.AddListener(Delete);
        }

        public void Focus(StoreData data)
        {
            _group.interactable = true;
            _focusedSave = data;
        }

        void EnterStore() => StoreData.SetCurrentStore(_focusedSave);

        void Delete() => StoreSerializer.Delete(_focusedSave);
    }
}
