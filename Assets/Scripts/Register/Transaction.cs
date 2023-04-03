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

        void RecalculateTotal()
        {
            var total = 0.0f;

            foreach (var li in _lineItems)
            {
                total += li.Price;
            }

            Total = total;

            TotalChanged?.Invoke(total);
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
                return;
            }

            AddDirectly(newItem);
        }
        
        void AddDirectly(LineItem lineItem)
        {
            _lineItems.Add(lineItem);

            lineItem.Transaction = this;

            lineItem.Updated += RecalculateTotal;

            LineItemAdded?.Invoke(lineItem);

            RecalculateTotal();
        }

        public void Remove(LineItem lineItem)
        {
            _lineItems.Remove(lineItem);

            lineItem.Updated -= RecalculateTotal;

            RecalculateTotal();
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

        public Transaction Transaction { get; set; }
        public float Price { get; private set; }


        public event Action Updated;

        public event Action Deleted;

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;

                Update();

                if (_quantity == 0)
                {
                    Delete();
                }
            }
        }

        public float UnitPrice => _overrideUnitPrice ?? Product.Price;
        public bool UnitPriceOverrideIsActive => _overrideUnitPrice != null;

        int _quantity;

        float? _overrideUnitPrice = null;

        public void OverrideUnitPrice(float overrideUnitPrice)
        {
            _overrideUnitPrice = overrideUnitPrice;
            Update();
        }

        void Update()
        {
            Price = UnitPrice * _quantity;
            Updated?.Invoke();
        }

        public void ClearOverrideUnitPrice()
        {
            _overrideUnitPrice = null;
            Update();
        }

        public LineItem(Product product, int quantity = 1)
        {
            Product = product;
            Quantity = quantity;
        }

        public void Delete()
        {
            Transaction.Remove(this);
            Deleted?.Invoke();
        }
    }
}
