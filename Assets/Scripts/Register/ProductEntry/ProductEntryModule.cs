using Recounter.Inventory;
using System;
using UnityEngine;

namespace Recounter.Service
{
    public abstract class ProductEntryModule : MonoBehaviour
    {
        public event Action<Product> ProductEntered;

        protected void EnterProduct(Product product) => ProductEntered?.Invoke(product);
    }
}
