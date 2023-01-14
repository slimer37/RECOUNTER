using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] Graphic[] activeGraphics;
    [SerializeField] RawImage itemThumb;
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
        foreach (var graphic in activeGraphics)
        {
            graphic.enabled = active;
        }

        if (!active || !punchTween.IsActive()) return;
        punchTween.Restart();
    }

    public void AssignItem(Item item)
    {
        if (!item)
            throw new System.NullReferenceException("Cannot assign null item to slot.");

        contents = item;
        itemThumb.enabled = true;
        itemThumb.texture = item.Thumbnail;
    }

    public void Clear()
    {
        contents = null;
        itemThumb.enabled = false;
    }
}