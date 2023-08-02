using Recounter.Store;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter.UI
{
    public class LoadSaveMenu : MonoBehaviour
    {
        [SerializeField] Transform _parent;
        [SerializeField] SaveListItem _listItemPrefab;
        [SerializeField] MenuEscape _menuEscape;

        readonly List<SaveListItem> _listItems = new();

        void Awake()
        {
            _menuEscape.Opened += LoadSaves;
        }

        void LoadSaves()
        {
            foreach (var item in _listItems)
            {
                Destroy(item.gameObject);
            }

            _listItems.Clear();

            _listItemPrefab.gameObject.SetActive(true);

            foreach (var fileName in StoreSerializer.AllSaveFiles())
            {
                if (!StoreSerializer.TryLoadSavedStore(fileName, out var storeData)) continue;

                var clone = Instantiate(_listItemPrefab, _parent);

                clone.Populate(storeData);

                _listItems.Add(clone);
            }

            _listItemPrefab.gameObject.SetActive(false);
        }

        public void SelectSave(StoreData save)
        {
            StoreData.SetCurrentStore(save);
        }
    }
}
