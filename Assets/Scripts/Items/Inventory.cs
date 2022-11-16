using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] Placer placer;
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] Transform slotParent;

    readonly List<InventorySlot> slots = new();

    readonly List<Item> items = new();

    InventorySlot activeSlot;

    public static Inventory Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        slotPrefab.gameObject.SetActive(true);

        for (int i = 0; i < capacity; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }

        slotPrefab.gameObject.SetActive(false);

        SetActiveSlot(0);

        Keyboard.current.onTextInput += OnSwitchSlot;
    }

    private void OnSwitchSlot(char input)
    {
        if (!int.TryParse(input.ToString(), out var slotNum)) return;

        SetActiveSlot(slotNum - 1);
    }

    public bool TryAddItem(Item item)
    {
        if (!item)
            throw new System.NullReferenceException();

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
            throw new System.NullReferenceException();

        if (!items.Remove(item))
            throw new System.Exception($"Cannot remove '{item}' because it's not in the inventory.");

        slots[items.Count].Clear();

        if (placer.Active == item)
            placer.StopHoldingItem();
    }

    void SetActiveSlot(int index)
    {
        placer.StopHoldingItem();

        activeSlot?.SetSlotActive(false);

        slots[index].SetSlotActive(true);
        activeSlot = slots[index];

        var item = slots[index].Item;

        if (item)
            placer.SetItem(item);
    }
}
