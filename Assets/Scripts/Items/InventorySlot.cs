using TMPro;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    Item contents;

    void Awake()
    {
        Clear();
    }

    public void AssignItem(Item item)
    {
        if (!item)
            throw new System.NullReferenceException("Cannot assign null item to slot.");

        contents = item;
        text.text = contents.gameObject.name;
    }

    public void Clear()
    {
        contents = null;
        text.text = "";
    }
}