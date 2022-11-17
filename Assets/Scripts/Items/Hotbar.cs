using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hotbar : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] Placer placer;
    [SerializeField] HotbarSlot slotPrefab;
    [SerializeField] Transform slotParent;

    readonly List<HotbarSlot> slots = new();

    readonly List<Item> items = new();

    int activeIndex;

    void Awake()
    {
        slotPrefab.gameObject.SetActive(true);

        for (int i = 0; i < capacity; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }

        slotPrefab.gameObject.SetActive(false);

        SetActiveSlot(0);
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

        if (items.Count == capacity) return false;

        var index = items.Count;
        slots[index].AssignItem(item);
        items.Add(item);

        SetActiveSlot(index);

        return true;
    }

    public void RemoveItem(Item item)
    {
        if (!item)
            throw new NullReferenceException();

        if (!items.Remove(item))
            throw new Exception($"Cannot remove '{item}' because it's not in the inventory.");

        slots[items.Count].Clear();

        if (placer.Active == item)
            placer.StopHoldingItem();
    }

    void SetActiveSlot(int index)
    {
        placer.StopHoldingItem();

        slots[activeIndex].SetSlotActive(false);

        slots[index].SetSlotActive(true);

        activeIndex = index;

        var item = slots[index].Item;

        if (item)
            placer.SetItem(item);
    }
}
