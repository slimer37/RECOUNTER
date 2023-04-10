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

        void OnValidate()
        {
            if (_totalInfo)
            {
                UpdateTotal();
            }
        }

        void EnableTransactionButtons(bool enabled)
        {
            _voidButton.interactable = enabled;
            _paymentButton.interactable = enabled;
            _discountFlatButton.interactable = enabled;
            _discountPercentButton.interactable = enabled;
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

            EnableTransactionButtons(false);
        }

        void PayWithCash()
        {
            EnableTransactionButtons(false);

            _cashPayment.Initiate(FinishPayment);
        }

        void FinishPayment()
        {
            // TODO: Continue transaction on customer side and do extra processing with transaction.
            VoidTransaction();
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

            EnableTransactionButtons(true);
        }

        void PromptVoidTransaction()
        {
            _confirmationPrompt.PromptYesNo("Void", "Are you sure you want to void this transaction?", VoidTransaction);
        }

        void VoidTransaction()
        {
            _currentTransaction.Void();
            _currentTransaction = null;

            EnableTransactionButtons(false);
        }

        void CreateLineItemUI(LineItem lineItem)
        {
            var clone = Instantiate(_lineItemPrefab, _listParent);
            clone.Initialize(lineItem);
            clone.gameObject.SetActive(true);
        }

        void DiscountFlat() => _numberEntry.PromptNumber(d => _currentTransaction.FlatDiscount = d);

        void DiscountPercent() => _numberEntry.PromptNumber(d => _currentTransaction.PercentDiscount = d, null, "P", 1, 4);

        void UpdateTotal()
        {
            if (_currentTransaction == null)
            {
                _totalInfo.text = string.Format(_totalFormat, 0, "", 0);
                return;
            }

            var percentDiscount = _currentTransaction.PercentDiscount;
            var flatDiscount = _currentTransaction.FlatDiscount;

            var discountString = "";

            if (percentDiscount > 0) discountString += percentDiscount.ToString("P2");

            if (flatDiscount > 0)
            {
                if (percentDiscount > 0) discountString += " + ";

                discountString += flatDiscount.ToString("C");
            }

            var extraInfo = "";

            if (!string.IsNullOrEmpty(discountString))
            {
                extraInfo = string.Format(_extraInfoFormat, discountString);
            }

            _totalInfo.text = string.Format(_totalFormat, _currentTransaction.Subtotal, extraInfo, _currentTransaction.Total);
        }
    }
}
