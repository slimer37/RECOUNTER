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

    const string SlotSelectChars = "1234567890";
    const string AltSlotSelectChars = "!@#$%^&*()";

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

    void OnSwitchSlot(char input)
    {
        // Check number char set for slot index.
        // Alternative char set used for if player is holding shift, i.e., ! -> 1 and @ -> 2

        int slotIndex = SlotSelectChars.IndexOf(input);

        if (slotIndex < 0) slotIndex = AltSlotSelectChars.IndexOf(input);

        if (slotIndex < 0) return;

        if (slotIndex < 0 || slotIndex >= capacity) return;

        SetActiveSlot(slotIndex);
    }

    public bool TryAddItem(Item item)
    {
        if (!item)
            throw new NullReferenceException();

        if (ActiveSlot.Item || items.Count == capacity) return false;

        slots[activeIndex].AssignItem(item);

        items.Add(item);

        SetActiveSlot(activeIndex, false, true);

        item.PickUp(this);

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
