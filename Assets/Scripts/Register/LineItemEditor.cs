using Recounter.Service;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    public class LineItemEditor : MonoBehaviour
    {
        [SerializeField] NumberEntry _numEntry;
        [SerializeField] Button _changeQuantity;

        LineItem _target;

        Action _switchedSelection;

        void Awake()
        {
            _changeQuantity.onClick.AddListener(ChangeQuantity);
            _changeQuantity.interactable = false;
        }

        public void Select(LineItem lineItem, Action switched)
        {
            _changeQuantity.interactable = true;

            _switchedSelection?.Invoke();
            _target = lineItem;
            _switchedSelection = switched;
        }

        void ChangeQuantity() => _numEntry.PromptNumber(ChangeQuantity, () => { }, "0", 1000, 100);

        void ChangeQuantity(float qty) => _target.Quantity = Mathf.RoundToInt(qty);
    }
}
