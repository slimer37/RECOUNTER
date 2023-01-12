using Recounter.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter.Tablet
{
    public class Cart : MonoBehaviour
    {
        [SerializeField] CartEntry _listItem;
        [SerializeField] Transform _listParent;

        readonly List<CartEntry> _contents = new();

        void Awake() => _listItem.gameObject.SetActive(false);

        public void Add(Product product, int quantity)
        {
            foreach (var entry in _contents)
            {
                if (entry.Product == product)
                {
                    entry.Quantity += quantity;
                    return;
                }
            }

            var entryClone = Instantiate(_listItem, _listParent);
            entryClone.gameObject.SetActive(true);
            entryClone.Product = product;
            entryClone.Quantity = quantity;
            _contents.Add(entryClone);
        }

        public void Remove(CartEntry entry) => _contents.Remove(entry);
    }
}
