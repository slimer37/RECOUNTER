using slimer37.Serialization;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.UI
{
    public class SaveListItem : MonoBehaviour
    {
        [SerializeField] TMP_Text _name;
        [SerializeField] TMP_Text _fileInfo;
        [SerializeField] Button _button;
        [SerializeField] LoadSaveMenu _menu;
        [SerializeField] Color _selectedColor;
        [SerializeField] Image _background;

        Color _deselectedColor;

        public SaveData Data { get; private set; }

        public event Action Removed;

        void Awake()
        {
            _button.onClick.AddListener(() => _menu.SelectSave(this));
            _deselectedColor = _background.color;
        }

        void OnDestroy()
        {
            if (Data == null) return;

            Data.Deleted -= OnDeleted;
        }

        public void Populate(SaveData storeData)
        {
            Data = storeData;

            Data.Deleted += OnDeleted;

            _name.text = Data.name;
            _fileInfo.text = $"[{Data.FullFileName}]\n{Data.creationTime:g}";
        }

        void OnDeleted()
        {
            Removed?.Invoke();
            Destroy(gameObject);
        }

        public void Select() => _background.color = _selectedColor;
        public void Deselect() => _background.color = _deselectedColor;
    }
}
