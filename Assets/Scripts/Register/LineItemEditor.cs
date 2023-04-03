using System;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Service
{
    public class LineItemEditor : MonoBehaviour
    {
        [SerializeField] NumberEntry _numEntry;
        [SerializeField] Transform _listParent;

        [Header("Buttons")]
        [SerializeField] CanvasGroup _buttonGroup;
        [SerializeField] Button _changeQuantity;
        [SerializeField] Button _changePrice;
        [SerializeField] Button _resetPrice;
        [SerializeField] Button _deleteButton;

        LineItem _target;

        Action _switchedSelection;

        void Awake()
        {
            _changeQuantity.onClick.AddListener(PromptChangeQuantity);
            _deleteButton.onClick.AddListener(Delete);
            _changePrice.onClick.AddListener(PromptChangePrice);
            _resetPrice.onClick.AddListener(ResetPrice);

            _resetPrice.interactable = false;
            _buttonGroup.interactable = false;
        }

        public void Select(LineItem lineItem, Action switched)
        {
            if (_target == lineItem) return;

            if (_target != null)
            {
                _target.Deleted -= UnsetTargetLineItem;
            }

            _target = lineItem;

            _target.Deleted += UnsetTargetLineItem;

            _switchedSelection?.Invoke();

            _switchedSelection = switched;

            _resetPrice.interactable = lineItem.UnitPriceOverrideIsActive;

            _buttonGroup.interactable = true;
        }

        void PromptChangeQuantity() => _numEntry.PromptNumber(ChangeQuantity, () => { }, "0", 1000, 0);
        void ChangeQuantity(float qty) => _target.Quantity = Mathf.RoundToInt(qty);

        void PromptChangePrice() => _numEntry.PromptNumber(ChangePrice, () => { });

        void ChangePrice(float overridePrice)
        {
            _target.OverrideUnitPrice(overridePrice);

            _resetPrice.interactable = _target.UnitPriceOverrideIsActive;
        }

        void Delete()
        {
            _target.Delete();
        }

        void ResetPrice()
        {
            _target.ClearOverrideUnitPrice();
            _resetPrice.interactable = false;
        }

        void UnsetTargetLineItem()
        {
            if (_target == null)
            {
                throw new NullReferenceException("Cannot unset null target line item.");
            }

            var transaction = _target.Transaction;
            var oldTarget = _target;

            _target = null;
            _switchedSelection = null;

            if (transaction.LineItems.Count > 0)
            {
                // Select last line item
                var last = _listParent.GetChild(_listParent.childCount - 1).GetComponent<LineItemUI>();

                if (last.LinkedLineItem != oldTarget)
                {
                    last.Select();
                }
                else
                {
                    _listParent.GetChild(_listParent.childCount - 2).GetComponent<LineItemUI>().Select();
                }
            }

            if (_target != null) return;

            _buttonGroup.interactable = false;
        }
    }
}
