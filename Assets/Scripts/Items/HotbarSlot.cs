using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image activeImage;
    [SerializeField] Vector3 punch;
    [SerializeField] float punchDuration;

    Item contents;
    Tween punchTween;

    public Item Item => contents;

    void Awake()
    {
        Clear();
        SetSlotActive(false);
    }

    void Start()
    {
        punchTween = transform.DOPunchPosition(punch, punchDuration)
            .SetAutoKill(false).Pause();
    }

    public void SetSlotActive(bool active)
    {
        activeImage.enabled = active;
        text.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;

        if (!active) return;
        punchTween.Restart();
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