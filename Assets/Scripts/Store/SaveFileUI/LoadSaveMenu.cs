using Recounter.Store;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Recounter.UI
{
    public class LoadSaveMenu : MonoBehaviour
    {
        [SerializeField] Transform _parent;
        [SerializeField] SaveListItem _listItemPrefab;
        [SerializeField] MenuEscape _menuEscape;
        [SerializeField] SaveInfoPanel _infoPanel;

        readonly List<SaveListItem> _listItems = new();

        SaveListItem _selectedListItem;

        public void OpenInExplorer()
        {
            Process.Start(StoreSerializer.GetSaveDirectory());
        }

        void Awake()
        {
            _menuEscape.Opened += LoadSaves;
        }

        void LoadSaves()
        {
            _selectedListItem = null;

            _infoPanel.ResetFocus();

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

                clone.Removed += () => _listItems.Remove(clone);
            }

            _listItemPrefab.gameObject.SetActive(false);
        }

        public void SelectSave(SaveListItem listItem)
        {
            if (_selectedListItem == listItem) return;

            listItem.Select();

            if (_selectedListItem)
                _selectedListItem.Deselect();

            _selectedListItem = listItem;

            _infoPanel.Focus(listItem.Data);
        }
    }
}
