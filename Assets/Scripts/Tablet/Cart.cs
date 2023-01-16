using Recounter.Delivery;
using Recounter.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class Cart : MonoBehaviour
    {
        [SerializeField] CartEntry _listItem;
        [SerializeField] Transform _listParent;
        [SerializeField] Button _checkoutButton;
        [SerializeField] DeliverySystem _deliverySystem;

        readonly List<CartEntry> _entries = new();

        void Awake()
        {
            _listItem.gameObject.SetActive(false);
            _checkoutButton.onClick.AddListener(Checkout);
            _checkoutButton.interactable = false;
        }

        public void Add(Product product, int quantity)
        {
            foreach (var entry in _entries)
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
            _entries.Add(entryClone);

            _checkoutButton.interactable = true;
        }

        public void Remove(CartEntry entry) => _entries.Remove(entry);

        public void Checkout()
        {
            var products = new List<Product>();

            foreach (var entry in _entries)
            {
                for (int i = 0; i < entry.Quantity; i++)
                {
                    products.Add(entry.Product);
                }

                Destroy(entry.gameObject);
            }

            _entries.Clear();

            _deliverySystem.Deliver(new Shipment(products));

            _checkoutButton.interactable = false;
        }
    }
}
