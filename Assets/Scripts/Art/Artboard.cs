using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Texture")]
    [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
    [SerializeField] Color backgroundColor;
    [SerializeField] RawImage image;
    [SerializeField] TextureFormat format;
    [SerializeField] FilterMode mode;

    [Header("Brush")]
    [SerializeField] ColorPicker colorPicker;
    [SerializeField] Slider radiusSlider;
    [SerializeField] TextMeshProUGUI radiusDisplay;

    int radius;
    Texture2D texture;
    Vector2Int lastDrawPosition;

    void Awake()
    {
        SetThickness(radiusSlider.value);
        radiusSlider.onValueChanged.AddListener(SetThickness);

        texture = new Texture2D(resolution.x, resolution.y, format, -1, false);
        texture.filterMode = mode;

        Clear();

        image.texture = texture;
    }

    void SetThickness(float v)
    {
        radius = (int)v;

        if (!radiusDisplay) return;
        radiusDisplay.text = radius.ToString();
    }

    void Clear()
    {
        var colors = texture.GetPixels();

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = backgroundColor;
        }

        texture.SetPixels(colors);
        texture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastDrawPosition = GetBrushPosition(eventData.position);

        Draw(lastDrawPosition.x, lastDrawPosition.y);

        texture.Apply();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var brushPosition = GetBrushPosition(eventData.position);

        DrawLine(lastDrawPosition.x, lastDrawPosition.y, brushPosition.x, brushPosition.y);

        lastDrawPosition = brushPosition;

        texture.Apply();
    }

    Vector2Int GetBrushPosition(Vector2 mousePosition)
    {
        var brushCenter = image.rectTransform.InverseTransformPoint(mousePosition);

        var rect = image.rectTransform.rect;

        brushCenter.x += rect.width / 2;
        brushCenter.y += rect.height / 2;

        brushCenter.x *= resolution.x / rect.width;
        brushCenter.y *= resolution.y / rect.height;

        var brushCenterInt = Vector2Int.FloorToInt(brushCenter);

        return brushCenterInt;
    }

    void DrawLine(int x0, int y0, int x1, int y1)
    {
        var dy = y1 - y0;
        var dx = x1 - x0;
        int stepX, stepY;

        if (dy < 0)
        {
            dy = -dy;
            stepY = -1;
        }
        else stepY = 1;
        if (dx < 0)
        {
            dx = -dx;
            stepX = -1;
        }
        else stepX = 1;
        dy <<= 1;
        dx <<= 1;

        float fraction;

        Draw(x0, y0);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepY;
                    fraction -= dx;
                }
                x0 += stepX;
                fraction += dy;
                Draw(x0, y0);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepX;
                    fraction -= dy;
                }
                y0 += stepY;
                fraction += dx;
                Draw(x0, y0);
            }
        }
    }

    void Draw(int x, int y)
    {
        var color = colorPicker.Color;

        if (radius == 1)
        {
            texture.SetPixel(x, y, color);
            return;
        }

        var r = radius - 1;

        var xMin = Mathf.Clamp(x - r, 0, texture.width);
        var xMax = Mathf.Clamp(x + r, 0, texture.width);
        var yMin = Mathf.Clamp(y - r, 0, texture.height);
        var yMax = Mathf.Clamp(y + r, 0, texture.height);
        var brushRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

        var size = Vector2Int.FloorToInt(brushRect.size);

        if (size.x == 0 || size.y == 0) return;

        var brush = new Color32[size.x * size.y];
        for (var i = 0; i < brush.Length; i++)
        {
            brush[i] = color;
        }

        texture.SetPixels32(xMin, yMin, size.x, size.y, brush);
    }
}
