using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter.Items
{
    public class Hotbar : MonoBehaviour
    {
        [SerializeField] int _capacity;
        [SerializeField] HotbarSlot _slotPrefab;
        [SerializeField] Transform _slotParent;

        const string SlotSelectChars = "1234567890";
        const string AltSlotSelectChars = "!@#$%^&*()";

        readonly List<HotbarSlot> _slots = new();

        readonly List<Item> _items = new();

        int _activeIndex;

        HotbarSlot ActiveSlot => _slots[_activeIndex];

        public bool IsActiveSlotFull => ActiveSlot.Item;

        public event ItemActiveHandler ItemBecameActive;
        public event ItemPutAwayHandler ItemPutAway;

        public delegate void ItemPutAwayHandler(Item item, bool wasItemKept);
        public delegate void ItemActiveHandler(Item item, bool fromInventory);

        void Awake()
        {
            _slotPrefab.gameObject.SetActive(true);

            for (var i = 0; i < _capacity; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotParent);
                _slots.Add(slot);
            }

            _slotPrefab.gameObject.SetActive(false);

            SetActiveSlot(0, force: true);
        }

        void OnEnable() => Keyboard.current.onTextInput += OnSwitchSlot;
        void OnDisable() => Keyboard.current.onTextInput -= OnSwitchSlot;

        void Update()
        {
            var scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll != 0)
            {
                var newIndex = _activeIndex + (scroll > 0 ? -1 : 1);
                newIndex = Mathf.Clamp(newIndex, 0, _capacity - 1);
                SetActiveSlot(newIndex);
            }
        }

        void OnSwitchSlot(char input)
        {
            // Check number char set for slot index.
            // Alternative char set used for if player is holding shift, i.e., ! -> 1 and @ -> 2

            var slotIndex = SlotSelectChars.IndexOf(input);

            if (slotIndex < 0) slotIndex = AltSlotSelectChars.IndexOf(input);

            if (slotIndex < 0) return;

            if (slotIndex < 0 || slotIndex >= _capacity) return;

            SetActiveSlot(slotIndex);
        }

        public bool TryAddItem(Item item)
        {
            if (!item)
                throw new NullReferenceException();

            if (ActiveSlot.Item || _items.Count == _capacity) return false;

            _slots[_activeIndex].AssignItem(item);

            _items.Add(item);

            SetActiveSlot(_activeIndex, false, true);

            item.PostPickUp(this);

            return true;
        }

        public bool TryRemoveActiveItem(out Item item)
        {
            item = null;

            if (!ActiveSlot.Item)
                return false;

            item = ActiveSlot.Item;

            RemoveItemFromSlot(ActiveSlot);

            return true;
        }

        public void RemoveItem(Item item)
        {
            if (!item)
                throw new NullReferenceException();

            var slot = _slots.Find(s => s.Item == item);

            if (!slot)
                throw new ArgumentException("Item is not in the hotbar.");

            RemoveItemFromSlot(slot);
        }

        void RemoveItemFromSlot(HotbarSlot slot)
        {
            var item = slot.Item;

            if (!_items.Remove(item))
                throw new Exception($"Cannot remove '{item}' because it's not in the inventory.");

            slot.Clear();

            if (ActiveSlot == slot)
            {
                ItemPutAway?.Invoke(item, false);
            }
        }

        void SetActiveSlot(int index, bool canResetPosition = true, bool force = false)
        {
            if (!force && (_activeIndex == index || Pause.IsPaused)) return;

            if (_activeIndex != index)
            {
                var previouslyActiveSlot = _slots[_activeIndex];

                previouslyActiveSlot.SetSlotActive(false);

                if (previouslyActiveSlot.Item)
                {
                    ItemPutAway?.Invoke(previouslyActiveSlot.Item, true);
                }
            }

            _slots[index].SetSlotActive(true);

            _activeIndex = index;

            var activeItem = _slots[index].Item;

            if (activeItem)
            {
                ItemBecameActive?.Invoke(activeItem, canResetPosition);
            }
        }
    }
}