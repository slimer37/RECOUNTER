using Recounter.Inventory;
using TMPro;
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
        // Card payment
        // Receipt printing

        [Header("Components")]
        [SerializeField] ProductEntryModule _productEntryModule;
        [SerializeField] NumberEntry _numberEntry;
        [SerializeField] DialogBox _confirmationPrompt;

        [Header("Transaction Operation Buttons")]
        [SerializeField] Button _discountFlatButton;
        [SerializeField] Button _discountPercentButton;
        [SerializeField] Button _voidButton;

        [Header("Payment")]
        [SerializeField] PaymentMethod _cashPayment;
        [SerializeField] Button _paymentButton;

        [Header("UI")]
        [SerializeField] TMP_Text _totalInfo;
        [SerializeField, TextArea] string _totalFormat = "Sub-total: {0:C}\n\n{1}\n\nTotal: {2:C}";
        [SerializeField, TextArea] string _extraInfoFormat = "Discount: {0}";

        [Header("Line Item UI")]
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

            _voidButton.onClick.AddListener(PromptVoidTransaction);
            _discountFlatButton.onClick.AddListener(DiscountFlat);
            _discountPercentButton.onClick.AddListener(DiscountPercent);

            _paymentButton.onClick.AddListener(PayWithCash);

            _lineItemPrefab.gameObject.SetActive(false);

            _voidButton.interactable = false;
            _paymentButton.interactable = false;
        }

        void PayWithCash()
        {
            _voidButton.interactable = false;
            _paymentButton.interactable = false;
            _cashPayment.Initiate(FinishPayment);
        }

        void FinishPayment()
        {
            // TODO: Continue transaction on customer side and do extra processing with transaction.
            VoidTransaction();
        }

        void DiscountFlat()
        {
            _numberEntry.PromptNumber(SetFlatDiscount);
        }

        void DiscountPercent()
        {
            _numberEntry.PromptNumber(SetPercentDiscount, null, "P", 1, 4);
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

            if (_currentTransaction == null) BeginTransaction();

            _currentTransaction.Add(lineItem);
        }

        void BeginTransaction()
        {
            _currentTransaction = new Transaction(CreateLineItemUI, UpdateTotal);
            _voidButton.interactable = true;
            _paymentButton.interactable = true;
        }

        void PromptVoidTransaction()
        {
            _confirmationPrompt.PromptYesNo("Void", "Are you sure you want to void this transaction?", VoidTransaction);
        }

        void VoidTransaction()
        {
            _currentTransaction.Void();
            _currentTransaction = null;

            _voidButton.interactable = false;
            _paymentButton.interactable = false;
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
