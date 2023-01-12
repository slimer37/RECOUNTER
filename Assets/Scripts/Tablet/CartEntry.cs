using Recounter.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class CartEntry : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _quantityText;
        [SerializeField] string _quantityFormat = "{0}";
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] RawImage _thumbnail;
        [SerializeField] Cart _cart;

        int _quantity;
        Product _product;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                _thumbnail.texture = _product.Thumbnail;
                _nameText.text = _product.DisplayName;
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                _quantityText.text = string.Format(_quantityFormat, _quantity);
            }
        }

        public void Remove()
        {
            _cart.Remove(this);
            Destroy(gameObject);
        }
    }
}