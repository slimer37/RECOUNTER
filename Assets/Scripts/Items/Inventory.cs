using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] int capacity;
    [SerializeField] Placer placer;

    List<Item> items = new();

    public static Inventory Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public bool TryAddItem(Item item)
    {
        if (items.Count == capacity) return false;

        items.Add(item);
        SetActiveItem(item);

        return true;
    }

    void SetActiveItem(Item item)
    {
        placer.SetItem(item);
    }
}
