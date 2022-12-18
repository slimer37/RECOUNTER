using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreviewBrush : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] Image image;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] float radiusOffset;
    [SerializeField] float radiusMultiplier;

    public float Radius { get; set; }
    public Color Color { get; set; }

    void Awake()
    {
        image.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
        rectTransform.sizeDelta = Vector2.one * (Radius + radiusOffset) * radiusMultiplier;
        image.color = Color;
        image.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.enabled = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }
}
