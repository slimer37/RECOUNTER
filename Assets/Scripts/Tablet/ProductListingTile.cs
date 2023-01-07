using Recounter.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class ProductListingTile : MonoBehaviour
    {
        [SerializeField] ProductListingUI _listingUI;
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] TextMeshProUGUI _priceText;
        [SerializeField] RawImage _image;

        public void InitializeToProduct(Product product)
        {
            _nameText.text = product.DisplayName;
            _priceText.text = product.FormattedPrice;
            _image.texture = product.Thumbnail;
        }
    }
}