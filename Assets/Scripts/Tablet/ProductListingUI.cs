using DG.Tweening;
using Recounter.Inventory;
using TMPro;
using UnityEngine;

namespace Recounter.Tablet
{
    public class ProductListingUI : MonoBehaviour
    {
        [SerializeField] Page _page;
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _price;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] ProductTurntable _turntable;

        [Header("Adding To Cart")]
        [SerializeField] Cart _cart;
        [SerializeField] RectTransform _addSuccessMessage;
        [SerializeField] Vector3 _punch;
        [SerializeField] float _duration;

        Product _focusedProduct;

        public void AddToCart()
        {
            _cart.Add(_focusedProduct, 1);
            _addSuccessMessage.gameObject.SetActive(true);
            _addSuccessMessage.DOComplete();
            _addSuccessMessage.DOPunchScale(_punch, _duration);
        }

        public void Open(Product product)
        {
            _addSuccessMessage.gameObject.SetActive(false);

            _page.Open();

            _focusedProduct = product;

            _name.text = product.DisplayName;
            _price.text = product.FormattedPrice;
            _description.text = product.Description;

            _turntable.Display(product);
        }
    }
}
