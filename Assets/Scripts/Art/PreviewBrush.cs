using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreviewBrush : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] Image image;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] float radiusOffset;
    [SerializeField] float radiusMultiplier;
    [SerializeField] Outline outline;

    public float Radius { get; set; }
    public Color Color { get; set; }

    void Awake()
    {
        image.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
        rectTransform.sizeDelta = (Radius + radiusOffset) * radiusMultiplier * Vector2.one;

        image.color = Color;
        Color.RGBToHSV(Color, out _, out _, out float v);
        outline.effectColor = Color.HSVToRGB(0, 0, 1 - v);

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
