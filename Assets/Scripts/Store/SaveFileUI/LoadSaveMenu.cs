using Recounter.Store;
using UnityEngine;

namespace Recounter.UI
{
    public class LoadSaveMenu : MonoBehaviour
    {
        [SerializeField] Transform _parent;
        [SerializeField] SaveListItem _listItemPrefab;

        void Awake()
        {
            _listItemPrefab.gameObject.SetActive(true);

            foreach (var fileName in StoreSerializer.AllSaveFiles())
            {
                if (!StoreSerializer.TryLoadSavedStore(fileName, out var storeData)) continue;

                var clone = Instantiate(_listItemPrefab, _parent);

                clone.Populate(storeData);
            }

            _listItemPrefab.gameObject.SetActive(false);
        }

        public void SelectSave(StoreData save)
        {
            StoreData.SetCurrentStore(save);
        }
    }
}
