using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreviewBrush : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] Image image;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Outline outline;
    [SerializeField] Slider radiusSlider;

    public Color Color { get; set; }

    void Awake()
    {
        image.enabled = false;

        UpdateRadius(radiusSlider.value);
        radiusSlider.onValueChanged.AddListener(UpdateRadius);
    }

    void UpdateRadius(float radius)
    {
        rectTransform.sizeDelta = radius * 2 * Vector2.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;

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
