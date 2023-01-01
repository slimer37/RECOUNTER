using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hotbar : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] OctoPlacer placer;
    [SerializeField] HotbarSlot slotPrefab;
    [SerializeField] Transform slotParent;

    readonly List<HotbarSlot> slots = new();

    readonly List<Item> items = new();

    int activeIndex;

    HotbarSlot ActiveSlot => slots[activeIndex];

    public bool IsActiveSlotFull => ActiveSlot.Item;

    void Awake()
    {
        slotPrefab.gameObject.SetActive(true);

        for (int i = 0; i < capacity; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }

        slotPrefab.gameObject.SetActive(false);

        SetActiveSlot(0, force: true);
    }

    void OnEnable() => Keyboard.current.onTextInput += OnSwitchSlot;
    void OnDisable() => Keyboard.current.onTextInput -= OnSwitchSlot;

    void Update()
    {
        var scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            var newIndex = activeIndex + (scroll > 0 ? -1 : 1);
            newIndex = Mathf.Clamp(newIndex, 0, capacity - 1);
            SetActiveSlot(newIndex);
        }
    }

    private void OnSwitchSlot(char input)
    {
        if (!int.TryParse(input.ToString(), out var slotNum)) return;

        if (slotNum == 0 || slotNum > capacity) return;

        SetActiveSlot(slotNum - 1);
    }

    public bool TryAddItem(Item item)
    {
        if (!item)
            throw new NullReferenceException();

        if (ActiveSlot.Item || items.Count == capacity) return false;

        slots[activeIndex].AssignItem(item);

        items.Add(item);

        SetActiveSlot(activeIndex, false, true);

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

        var slot = slots.Find(s => s.Item == item);

        if (!slot)
            throw new ArgumentException("Item is not in the hotbar.");

        RemoveItemFromSlot(slot);
    }

    void RemoveItemFromSlot(HotbarSlot slot)
    {
        var item = slot.Item;

        if (!items.Remove(item))
            throw new Exception($"Cannot remove '{item}' because it's not in the inventory.");

        slot.Clear();

        if (ActiveSlot == slot)
            placer.StopHoldingItem();
    }

    void SetActiveSlot(int index, bool canResetPosition = true, bool force = false)
    {
        if (!force && (activeIndex == index || placer.IsPlacing || Pause.IsPaused)) return;

        placer.StopHoldingItem();

        slots[activeIndex].SetSlotActive(false);

        slots[index].SetSlotActive(true);

        activeIndex = index;

        var item = slots[index].Item;

        if (item)
            placer.SetItem(item, canResetPosition);
    }
}
