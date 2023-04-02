using Recounter.Inventory;
using System;
using System.Collections.Generic;

namespace Recounter.Service
{
    public class Transaction
    {
        public IReadOnlyList<LineItem> LineItems => _lineItems.AsReadOnly();

        readonly List<LineItem> _lineItems = new();
        
        Action<LineItem> LineItemAdded;
        Action<float> TotalChanged;

        public float Total { get; private set; }

        float CalculateTotal()
        {
            var total = 0.0f;

            foreach (var li in _lineItems)
            {
                total += li.Price;
            }

            return Total = total;
        }

        bool ContainsProduct(Product product, out LineItem lineItem)
        {
            lineItem = null;

            foreach (var li in _lineItems)
            {
                if (li.Product == product)
                {
                    lineItem = li;
                    return true;
                }
            }

            return false;
        }

        public void Add(LineItem newItem)
        {
            if (ContainsProduct(newItem.Product, out var lineItem))
            {
                lineItem.Quantity += newItem.Quantity;

                TotalChanged?.Invoke(CalculateTotal());

                return;
            }

            AddDirectly(newItem);
        }
        
        void AddDirectly(LineItem lineItem)
        {
            _lineItems.Add(lineItem);
            LineItemAdded?.Invoke(lineItem);
            TotalChanged?.Invoke(CalculateTotal());
        }

        public static Transaction Create(LineItem lineItem, Action<LineItem> lineItemCallback, Action<float> totalChangeCallback)
        {
            var transaction = new Transaction
            {
                LineItemAdded = lineItemCallback,
                TotalChanged = totalChangeCallback
            };

            transaction.AddDirectly(lineItem);

            return transaction;
        }
    }

    public class LineItem
    {
        public readonly Product Product;

        public float Price { get; private set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;

                Price = Product.Price * _quantity;

                QuantityChanged?.Invoke(_quantity);
            }
        }

        int _quantity;

        public event Action<int> QuantityChanged;

        public LineItem(Product product, int quantity = 1)
        {
            Product = product;
            Quantity = quantity;
        }
    }
}
