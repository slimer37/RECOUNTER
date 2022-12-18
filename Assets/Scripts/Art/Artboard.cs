using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Texture")]
    [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
    [SerializeField] Color backgroundColor;
    [SerializeField] ComputeShader cs;
    [SerializeField] RawImage image;
    [SerializeField] FilterMode mode;

    [Header("Brush")]
    [SerializeField] ColorPicker colorPicker;
    [SerializeField] Slider radiusSlider;
    [SerializeField] TextMeshProUGUI radiusDisplay;

    int radius;
    RenderTexture texture;

    void Awake()
    {
        texture = new RenderTexture(resolution.x, resolution.y, 0);
        texture.enableRandomWrite = true;
        texture.filterMode = mode;

        cs.SetTexture(0, "Result", texture);
        cs.SetTexture(1, "Result", texture);
        cs.SetTexture(2, "Result", texture);
        cs.SetInts("Dimensions", resolution.x, resolution.y);

        SetColor(backgroundColor);

        Clear();

        SetThickness(radiusSlider.value);
        radiusSlider.onValueChanged.AddListener(SetThickness);

        SetColor(colorPicker.Color);
        colorPicker.onColorChanged.AddListener(SetColor);

        image.texture = texture;
    }

    void SetThickness(float v)
    {
        radius = (int)v;
        cs.SetFloat("Radius", radius);

        if (!radiusDisplay) return;
        radiusDisplay.text = radius.ToString();
    }

    public void Clear()
    {
        var c = backgroundColor;
        cs.Dispatch(0, texture.width / 8, texture.height / 8, 1);
    }

    void SetColor(Color color)
    {
        var c = colorPicker.Color;
        cs.SetFloats("Color", c.r, c.g, c.b, c.a);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var p = GetBrushPosition(eventData.position);

        if (Keyboard.current.shiftKey.isPressed)
        {
            DrawContinuousLine(p);
        }
        else
        {
            Draw(p);
        }
    }

    void Draw(Vector2 point)
    {
        cs.SetFloats("A", point.x, point.y);
        cs.Dispatch(1, texture.width / 8, texture.height / 8, 1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var p = GetBrushPosition(eventData.position);
        DrawContinuousLine(p);
    }

    void DrawContinuousLine(Vector2 next)
    {
        cs.SetFloats("B", next.x, next.y);
        cs.Dispatch(2, texture.width / 8, texture.height / 8, 1);
        cs.SetFloats("A", next.x, next.y);
    }

    Vector2 GetBrushPosition(Vector2 mousePosition)
    {
        var brushCenter = image.rectTransform.InverseTransformPoint(mousePosition);

        var rect = image.rectTransform.rect;

        brushCenter.x += rect.width / 2;
        brushCenter.y += rect.height / 2;

        brushCenter.x *= resolution.x / rect.width;
        brushCenter.y *= resolution.y / rect.height;

        return brushCenter;
    }
}
