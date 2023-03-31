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

        [SerializeField] TMP_Text _totaling;
        [SerializeReference] ProductEntryModule _productEntryModule;

        void Awake()
        {
            _productEntryModule.ProductEntered += OnProductEntered;
        }

        void OnProductEntered(Product product)
        {
            print("Entered " + product.DisplayName);
        }

        public void Pay()
        {

        }
    }
}
