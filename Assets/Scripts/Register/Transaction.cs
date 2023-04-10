using Recounter.Inventory;
using System;
using System.Collections.Generic;

namespace Recounter.Service
{
    public class Transaction
    {
        public IReadOnlyList<LineItem> LineItems => _lineItems.AsReadOnly();

        readonly List<LineItem> _lineItems = new();
        
        readonly Action<LineItem> _lineItemAdded;
        readonly Action _changed;

        public float Subtotal { get; private set; }
        public float Total { get; private set; }

        public float PercentDiscount
        {
            get => _percentDiscount;
            set
            {
                _percentDiscount = value;
                RecalculateTotal();
            }
        }

        float _percentDiscount;

        public float FlatDiscount
        {
            get => _flatDiscount;
            set
            {
                _flatDiscount = value;
                RecalculateTotal();
            }
        }

        float _flatDiscount;

        void RecalculateTotal()
        {
            var subtotal = 0.0f;

            foreach (var li in _lineItems)
            {
                subtotal += li.Price;
            }

            Subtotal = subtotal;

            Total = Subtotal;

            Total *= 1 - _percentDiscount;
            Total -= _flatDiscount;

            _changed.Invoke();
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

            _lineItemAdded.Invoke(lineItem);

            RecalculateTotal();
        }

        public void Remove(LineItem lineItem)
        {
            _lineItems.Remove(lineItem);

            lineItem.Updated -= RecalculateTotal;

            RecalculateTotal();
        }

        public Transaction(Action<LineItem> lineItemCallback, Action changeCallback)
        {
            _lineItemAdded = lineItemCallback;
            _changed = changeCallback;
        }

        public void Void()
        {
            foreach (var item in _lineItems.ToArray())
            {
                item.Delete();
            }
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

        public float OriginalUnitPrice => Product.Price;
        public float UnitPrice => _overrideUnitPrice ?? OriginalUnitPrice;
        public bool UnitPriceOverrideIsActive => _overrideUnitPrice != null;

        int _quantity;

        float? _overrideUnitPrice = null;

        public void OverrideUnitPrice(float overrideUnitPrice)
        {
            if (overrideUnitPrice == OriginalUnitPrice) return;

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
