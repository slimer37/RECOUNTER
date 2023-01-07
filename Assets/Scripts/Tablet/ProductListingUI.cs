using Recounter.Inventory;
using TMPro;
using UnityEngine;

namespace Recounter.Tablet
{
    public class ProductListingUI : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _price;
        [SerializeField] TextMeshProUGUI _description;

        Product _focusedProduct;

        void Open(Product product)
        {
            _focusedProduct = product;

            _name.text = product.DisplayName;
            _price.text = product.FormattedPrice;
            _description.text = product.Description;
        }
    }
}
