using Recounter.Inventory;
using System;
using System.Collections.Generic;

namespace Recounter.Service
{
    public class Transaction
    {
        public IReadOnlyList<LineItem> LineItems => _lineItems.AsReadOnly();

        List<LineItem> _lineItems = new();
        
        Action<LineItem> LineItemAdded;

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
                return;
            }

            AddDirectly(newItem);
        }
        
        void AddDirectly(LineItem lineItem)
        {
            _lineItems.Add(lineItem);
            LineItemAdded?.Invoke(lineItem);
        }

        public static Transaction Create(LineItem lineItem, Action<LineItem> lineItemCallback)
        {
            var transaction = new Transaction
            {
                LineItemAdded = lineItemCallback
            };

            transaction.AddDirectly(lineItem);

            return transaction;
        }
    }

    public class LineItem
    {
        public Product Product { get; private set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                QuantityChanged?.Invoke(_quantity);
            }
        }

        int _quantity;

        public event Action<int> QuantityChanged;

        public LineItem(Product product, int quantity = 1)
        {
            Product = product;
            _quantity = quantity;
        }
    }
}
