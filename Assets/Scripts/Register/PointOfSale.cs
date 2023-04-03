using Recounter.Inventory;
using TMPro;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Service
{
    public class PointOfSale : MonoBehaviour
    {
        // TODO:
        // Manually looking up items
        // Tax
        // Holding orders
        // Pay (Cash tender / card)
        // Receipt printing

        [SerializeField] TMP_Text _totalInfo;
        [SerializeField, TextArea] string _totalFormat = "Sub-total: {0:C}\n\n{1}\n\nTotal: {2:C}";
        [SerializeField, TextArea] string _extraInfoFormat = "Discount: {0}";

        [SerializeReference] ProductEntryModule _productEntryModule;
        [SerializeReference] NumberEntry _numberEntry;
        [SerializeReference] Button _discountFlatButton;
        [SerializeReference] Button _discountPercentButton;
        [SerializeReference] Button _voidButton;

        [SerializeField] LineItemUI _lineItemPrefab;
        [SerializeField] Transform _listParent;

        Transaction _currentTransaction;

        float _flatDiscount;
        float _percentDiscount;

        float _total;

        void OnValidate()
        {
            if (_totalInfo)
            {
                UpdateTotal();
            }
        }

        void Awake()
        {
            _productEntryModule.ProductEntered += OnProductEntered;

            UpdateTotal();

            _voidButton.onClick.AddListener(VoidTransaction);
            _discountFlatButton.onClick.AddListener(DiscountFlat);
            _discountPercentButton.onClick.AddListener(DiscountPercent);

            _lineItemPrefab.gameObject.SetActive(false);

            _voidButton.interactable = false;
        }

        void DiscountFlat()
        {
            _numberEntry.PromptNumber(SetFlatDiscount, () => { });
        }

        void DiscountPercent()
        {
            _numberEntry.PromptNumber(SetPercentDiscount, () => { }, "P", 100, 0.01f);
        }

        void SetFlatDiscount(float d)
        {
            _flatDiscount = d;
            UpdateTotal();
        }

        void SetPercentDiscount(float d)
        {
            _percentDiscount = d;
            UpdateTotal();
        }

        void OnProductEntered(Product product)
        {
            var lineItem = new LineItem(product);

            if (_currentTransaction == null)
            {
                BeginTransaction(lineItem);
            }
            else
            {
                _currentTransaction.Add(lineItem);
            }
        }

        void BeginTransaction(LineItem initial)
        {
            _currentTransaction = Transaction.Create(initial, CreateLineItemUI, UpdateTotal);
            _voidButton.interactable = true;
        }

        void VoidTransaction()
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;

            _voidButton.interactable = false;
        }

        void CreateLineItemUI(LineItem lineItem)
        {
            var clone = Instantiate(_lineItemPrefab, _listParent);
            clone.Initialize(lineItem);
            clone.gameObject.SetActive(true);
        }

        void UpdateTotal() => UpdateTotal(_currentTransaction?.Total ?? 0);

        void UpdateTotal(float subtotal)
        {
            _total = subtotal;

            var discountString = "";

            if (_percentDiscount > 0)
            {
                discountString += _percentDiscount.ToString("P2");
                _total *= 1 - _percentDiscount;
            }

            if (_flatDiscount > 0)
            {
                if (_percentDiscount > 0)
                {
                    discountString += " + ";
                }

                discountString += _flatDiscount.ToString("C");
                _total -= _flatDiscount;
            }

            var extraInfo = "";

            if (!string.IsNullOrEmpty(discountString))
            {
                extraInfo = string.Format(_extraInfoFormat, discountString);
            }

            _totalInfo.text = string.Format(_totalFormat, subtotal, extraInfo, _total);
        }
    }
}
