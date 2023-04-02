using TMPro;
using UnityEngine;

namespace Recounter.Service
{
    public class LineItemUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _info;
        [SerializeField] TMP_Text _price;
        [SerializeField] TMP_Text _quantity;

        LineItem _linkedLineItem;

        public void PopulateInfo(LineItem lineItem)
        {
            _linkedLineItem = lineItem;

            _info.text = lineItem.Product.DisplayName;
            _price.text = lineItem.Product.FormattedPrice;
            _quantity.text = lineItem.Quantity.ToString();

            lineItem.QuantityChanged += QuantityChanged;
        }

        void QuantityChanged(int quantity)
        {
            _quantity.text = quantity.ToString();
            _price.text = (_linkedLineItem.Product.Price * quantity).ToString("C");
        }
    }
}
