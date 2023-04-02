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
        [SerializeField, TextArea, RequireSubstring("{0}", "{1}")] string _totalFormat;

        [SerializeReference] ProductEntryModule _productEntryModule;

        [SerializeField] LineItemUI _lineItemPrefab;
        [SerializeField] Transform _listParent;

        Transaction _currentTransaction;

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

        void UpdateTotal(float total)
        {
            _totalInfo.text = string.Format(_totalFormat, total, total);
        }
    }
}
