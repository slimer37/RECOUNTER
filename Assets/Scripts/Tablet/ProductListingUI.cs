using Recounter.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class ProductListingUI : MonoBehaviour
    {
        [SerializeField] Canvas _canvas;
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _price;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] RawImage _image;

        Product _focusedProduct;

        void Awake()
        {
            _canvas.enabled = false;
        }

        public void Open(Product product)
        {
            _canvas.enabled = true;

            _focusedProduct = product;

            _name.text = product.DisplayName;
            _price.text = product.FormattedPrice;
            _description.text = product.Description;
            _image.texture = product.Thumbnail;
        }
    }
}
