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

        public StoreData Data { get; private set; }

        void Awake()
        {
            _button.onClick.AddListener(() => _menu.SelectSave(Data));
        }

        public void Populate(StoreData storeData)
        {
            Data = storeData;

            _name.text = Data.name;
            _fileInfo.text = $"[{Data.fileName}]\n{Data.creationTime:g}";
        }
    }
}
