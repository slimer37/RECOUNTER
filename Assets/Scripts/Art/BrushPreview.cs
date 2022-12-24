using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrushPreview : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] Artboard artboard;
    [SerializeField] Image preview;
    [SerializeField] RectTransform previewTransform;
    [SerializeField] Outline outline;

    Brush lastBrush;
    Vector2 scaleRatio;

    void Awake()
    {
        artboard.BrushSelected += OnBrushSelected;

        var dimensions = artboard.Resolution;
        var actual = (artboard.transform as RectTransform).rect.size;
        scaleRatio = new Vector2(actual.x / dimensions.x, actual.y / dimensions.y);

        OnBrushSelected(artboard.CurrentBrush);
    }

    void OnBrushSelected(Brush brush)
    {
        if (lastBrush)
        {
            lastBrush.RadiusChanged -= UpdateRadius;
            lastBrush.ColorChanged -= UpdateColor;
        }

        brush.RadiusChanged += UpdateRadius;
        brush.ColorChanged += UpdateColor;

        UpdateRadius(brush.Radius);
        UpdateColor(brush.Color);

        lastBrush = brush;
    }

    void UpdateRadius(float radius) => previewTransform.sizeDelta = radius * 2 * scaleRatio;

    void UpdateColor(Color color)
    {
        // Invert outline value based on current color
        preview.color = color;
        Color.RGBToHSV(color, out _, out _, out float v);
        outline.effectColor = Color.HSVToRGB(0, 0, 1 - v);
    }

    public void OnPointerMove(PointerEventData eventData) => previewTransform.position = eventData.position;

    public void OnPointerEnter(PointerEventData eventData) => preview.enabled = true;

    public void OnPointerExit(PointerEventData eventData) => preview.enabled = false;
}
