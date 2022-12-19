using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Texture")]
    [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
    [SerializeField] Color backgroundColor;
    [SerializeField] ComputeShader clearCs;
    [SerializeField] RawImage image;
    [SerializeField] FilterMode mode;

    [Header("Brush")]
    [SerializeField] Brush brush;

    RenderTexture texture;

    Vector2Int threadCount;
    int clearKernel;

    void Awake()
    {
        threadCount = new(resolution.x / 8, resolution.y / 8);
        clearKernel = clearCs.FindKernel("Clear");

        texture = new RenderTexture(resolution.x, resolution.y, 0)
        {
            enableRandomWrite = true,
            filterMode = mode
        };

        clearCs.SetTexture(0, "Result", texture);

        SetColor(backgroundColor);
        ClearBoard();

        image.texture = texture;

        SetBrush(brush);
    }

    public void SetBrush(Brush newBrush)
    {
        brush = newBrush;
        brush.InitializeWithTexture(texture);
    }

    void SetColor(Color c) => clearCs.SetFloats("Color", c.r, c.g, c.b, c.a);
    void ClearBoard() => clearCs.Dispatch(clearKernel, threadCount.x, threadCount.y, 1);

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

    void Draw(Vector2 point) => brush.Draw(point.x, point.y);

    public void OnDrag(PointerEventData eventData)
    {
        var p = GetBrushPosition(eventData.position);
        DrawContinuousLine(p);
    }

    void DrawContinuousLine(Vector2 next) => brush.DrawContinuousLine(next.x, next.y);

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
