using System;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Service
{
    public class LineItemEditor : MonoBehaviour
    {
        [SerializeField] NumberEntry _numEntry;
        [SerializeField] Button _changeQuantity;
        [SerializeField] Button _deleteButton;
        [SerializeField] Transform _listParent;

        LineItem _target;

        Action _switchedSelection;

        void Awake()
        {
            _changeQuantity.onClick.AddListener(ChangeQuantity);
            _deleteButton.onClick.AddListener(Delete);

            _changeQuantity.interactable = false;
            _deleteButton.interactable = false;
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

            _changeQuantity.interactable = true;
            _deleteButton.interactable = true;
        }

        void ChangeQuantity() => _numEntry.PromptNumber(ChangeQuantity, () => { }, "0", 1000, 100);

        void ChangeQuantity(float qty) => _target.Quantity = Mathf.RoundToInt(qty);

        void Delete()
        {
            _target.Delete();
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

            _changeQuantity.interactable = false;
            _deleteButton.interactable = false;
        }
    }
}
