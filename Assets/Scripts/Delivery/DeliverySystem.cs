using Recounter.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter.Delivery
{
    public class DeliverySystem : MonoBehaviour
    {
        [SerializeField] GameObject _boxPrefab;
        [SerializeField] Transform _spawn;

        public void Deliver(Shipment shipment)
        {
            var box = Instantiate(_boxPrefab, _spawn.position, Quaternion.identity).GetComponentInChildren<Box>();

            var items = new List<Item>();

            foreach (var product in shipment.Products)
            {
                var instance = Instantiate(product.Prefab);
                items.Add(instance.GetComponent<Item>());
            }

            box.Fill(items);
        }
    }

    public class Shipment
    {
        public readonly List<Product> Products;

        public Shipment(List<Product> products)
        {
            Products = products;
        }
    }
}
