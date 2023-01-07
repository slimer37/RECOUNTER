using Recounter.Inventory;
using TMPro;
using UnityEngine;

namespace Recounter.Tablet
{
    public class ProductListingTile : MonoBehaviour
    {
        [SerializeField] ProductListingUI _listingUI;
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] TextMeshProUGUI _priceText;
        [SerializeField] TextMeshProUGUI _descText;

        public void InitializeToProduct(Product product)
        {
            _nameText.text = product.DisplayName;
            _priceText.text = product.FormattedPrice;
            _descText.text = product.Description;
        }
    }
}