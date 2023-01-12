using Recounter.Inventory;
using TMPro;
using UnityEngine;

namespace Recounter.Tablet
{
    public class ProductListingUI : MonoBehaviour
    {
        [SerializeField] Canvas _canvas;
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _price;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] ProductTurntable _turntable;
        [SerializeField] Cart _cart;
        [SerializeField] GameObject _addSuccessMessage;
        [SerializeField] float _messageTimeout;

        Product _focusedProduct;

        void Awake()
        {
            Close();
        }

        public void AddToCart()
        {
            _cart.Add(_focusedProduct, 1);
            _addSuccessMessage.SetActive(true);

            CancelInvoke();
            Invoke(nameof(HideMessage), _messageTimeout);
        }

        void HideMessage()
        {
            _addSuccessMessage.SetActive(false);
        }

        public void Close()
        {
            _canvas.enabled = false;
            _turntable.enabled = false;
        }

        public void Open(Product product)
        {
            CancelInvoke();
            HideMessage();

            _turntable.enabled = true;
            _canvas.enabled = true;

            _focusedProduct = product;

            _name.text = product.DisplayName;
            _price.text = product.FormattedPrice;
            _description.text = product.Description;

            _turntable.Display(product);
        }
    }
}
