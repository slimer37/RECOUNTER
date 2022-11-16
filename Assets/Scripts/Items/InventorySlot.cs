using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image activeImage;

    Item contents;

    public Item Item => contents;

    void Awake()
    {
        Clear();
        SetSlotActive(false);
    }

    public void SetSlotActive(bool active)
    {
        activeImage.enabled = active;
        text.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
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