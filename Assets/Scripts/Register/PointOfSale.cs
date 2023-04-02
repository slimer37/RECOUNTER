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

        [SerializeReference] ProductEntryModule _productEntryModule;

        [SerializeField] LineItemUI _lineItemPrefab;
        [SerializeField] Transform _listParent;

        Transaction currentTransaction;

        void Awake()
        {
            _productEntryModule.ProductEntered += OnProductEntered;
        }

        void OnProductEntered(Product product)
        {
            var lineItem = new LineItem(product);

            if (currentTransaction == null)
            {
                currentTransaction = Transaction.Create(lineItem, CreateLineItemUI);
            }
            else
            {
                currentTransaction.Add(lineItem);
            }
        }

        void CreateLineItemUI(LineItem lineItem)
        {
            Instantiate(_lineItemPrefab, _listParent).PopulateInfo(lineItem);
        }
    }
}
