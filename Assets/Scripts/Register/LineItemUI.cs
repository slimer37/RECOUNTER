using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Service
{
    public class LineItemUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _info;
        [SerializeField] TMP_Text _price;
        [SerializeField] TMP_Text _quantity;
        [SerializeField] Graphic _focusGraphic;
        [SerializeField] Button _button;
        [SerializeField] LineItemEditor _editor;

        public LineItem LinkedLineItem => _linkedLineItem;

        LineItem _linkedLineItem;

        void Awake()
        {
            _button.onClick.AddListener(Select);
        }

        public void Select()
        {
            _editor.Select(_linkedLineItem, Deselect);
            _focusGraphic.enabled = true;
        }

        void Deselect()
        {
            _focusGraphic.enabled = false;
        }

        void OnDelete()
        {
            _linkedLineItem.QuantityChanged -= QuantityChanged;
            _linkedLineItem.Deleted -= OnDelete;

            Destroy(gameObject);
        }

        public void Initialize(LineItem lineItem)
        {
            _linkedLineItem = lineItem;

            _info.text = lineItem.Product.DisplayName;
            _price.text = lineItem.Product.FormattedPrice;
            _quantity.text = lineItem.Quantity.ToString();

            lineItem.QuantityChanged += QuantityChanged;
            lineItem.Deleted += OnDelete;

            Select();
        }

        void QuantityChanged(int quantity)
        {
            _quantity.text = quantity.ToString();
            _price.text = _linkedLineItem.Price.ToString("C");
        }
    }
}
