using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] Placer placer;
    [SerializeField] InventorySlot slotPrefab;
    [SerializeField] Transform slotParent;

    readonly List<InventorySlot> slots = new();

    readonly List<Item> items = new();

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
    }

    public bool TryAddItem(Item item)
    {
        if (!item)
            throw new System.NullReferenceException();

        if (items.Count == capacity) return false;

        slots[items.Count].AssignItem(item);
        items.Add(item);

        SetActiveItem(item);

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

    void SetActiveItem(Item item)
    {
        placer.StopHoldingItem();
        placer.SetItem(item);
    }
}
