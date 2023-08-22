using Recounter.Inventory;
using Recounter.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter.Delivery
{
    public class DeliverySystem : MonoBehaviour
    {
        [SerializeField] GameObject _boxPrefab;

        public void Deliver(Shipment shipment)
        {
            var box = Instantiate(_boxPrefab, Vector3.up * 5, Quaternion.identity).GetComponentInChildren<Box>();

            var items = new List<Placeable>();

            foreach (var product in shipment.Products)
            {
                var instance = Instantiate(product.Prefab);
                items.Add(instance.GetComponent<Placeable>());
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
