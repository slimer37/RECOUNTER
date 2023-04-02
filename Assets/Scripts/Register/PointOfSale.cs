using Recounter.Inventory;
using TMPro;
using UnityEngine;

namespace Recounter.Service
{
    public class PointOfSale : MonoBehaviour
    {
        // TODO:
        // Scanning and Manually looking up items
        // Totaling Sub-Total + Tax = Total
        // Holding orders
        // Pay (Cash tender / card)
        // Receipt printing

        [SerializeField] TMP_Text _totalInfo;
        [SerializeField, TextArea] string _totalFormat = "Sub-total: {0:C}\n\n{1}\n\nTotal: {2:C}";
        [SerializeField, TextArea] string _extraInfoFormat = "Discount: {0:P2}";

        [SerializeReference] ProductEntryModule _productEntryModule;

        [SerializeField] LineItemUI _lineItemPrefab;
        [SerializeField] Transform _listParent;

        Transaction _currentTransaction;

        float _discount;
        float _total;

        void OnValidate()
        {
            if (_totalInfo)
            {
                ResetTotal();
            }
        }

        void Awake()
        {
            _productEntryModule.ProductEntered += OnProductEntered;

            ResetTotal();
        }

        void ResetTotal() => UpdateTotal(0);

        void OnProductEntered(Product product)
        {
            var lineItem = new LineItem(product);

            if (_currentTransaction == null)
            {
                _currentTransaction = Transaction.Create(lineItem, CreateLineItemUI, UpdateTotal);
            }
            else
            {
                _currentTransaction.Add(lineItem);
            }
        }

        void CreateLineItemUI(LineItem lineItem)
        {
            Instantiate(_lineItemPrefab, _listParent).PopulateInfo(lineItem);
        }

        void UpdateTotal(float subtotal)
        {
            _total = subtotal * (1 - _discount);

            var extraInfo = "";

            if (_discount > 0)
            {
                extraInfo = string.Format(_extraInfoFormat, _discount);
            }

            _totalInfo.text = string.Format(_totalFormat, subtotal, extraInfo, _total);
        }
    }
}
