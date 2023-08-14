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
        [SerializeField, TextArea] string _format;
        
        string _emptyText;

        StoreData _focusedSave;

        public void ResetFocus()
        {
            _group.interactable = false;
            _focusedSave = null;
            _text.text = _emptyText;
        }

        void Awake()
        {
            _enter.onClick.AddListener(EnterStore);
            _delete.onClick.AddListener(PromptDeletion);

            _emptyText = _text.text;
        }

        public void Focus(StoreData data)
        {
            _group.interactable = true;
            _focusedSave = data;
            _text.text = string.Format(_format, data.name, data.creationTime.ToString("f"), data.FullFileName);
        }

        void EnterStore() => StoreData.SetCurrentStore(_focusedSave);

        void PromptDeletion()
        {
            Dialog.Main.Confirm(Delete, "Delete", $"Are you sure you want to delete \"{_focusedSave.name}\"");
        }

        void Delete()
        {
            _focusedSave.Delete();
            ResetFocus();
        }
    }
}
