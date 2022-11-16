using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] Placer placer;

    readonly List<Item> items = new();

    public static Inventory Instance { get; private set; }

    void Awake() => Instance = this;

    public bool TryAddItem(Item item)
    {
        if (items.Count == capacity) return false;

        items.Add(item);

        SetActiveItem(item);

        return true;
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);

        if (placer.Active == item)
            placer.StopHoldingItem();
    }

    void SetActiveItem(Item item)
    {
        placer.StopHoldingItem();
        placer.SetItem(item);
    }
}
