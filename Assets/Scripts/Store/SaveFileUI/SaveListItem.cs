using Recounter.Store;
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

        public StoreData Data { get; private set; }

        void Awake()
        {
            _button.onClick.AddListener(() => _menu.SelectSave(this));
            _deselectedColor = _background.color;
        }

        public void Populate(StoreData storeData)
        {
            Data = storeData;

            _name.text = Data.name;
            _fileInfo.text = $"[{Data.fileName}]\n{Data.creationTime:g}";
        }

        public void Select() => _background.color = _selectedColor;
        public void Deselect() => _background.color = _deselectedColor;
    }
}
