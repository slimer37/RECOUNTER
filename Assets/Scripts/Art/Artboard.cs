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
    [SerializeField] Brush brush;

    RenderTexture texture;

    Vector2Int threadCount;

    void Awake()
    {
        texture = new RenderTexture(resolution.x, resolution.y, 0)
        {
            enableRandomWrite = true,
            filterMode = mode
        };

        cs.SetTexture(0, "Result", texture);
        cs.SetTexture(1, "Result", texture);
        cs.SetTexture(2, "Result", texture);
        cs.SetInts("Dimensions", resolution.x, resolution.y);

        threadCount = new(resolution.x / 8, resolution.y / 8);

        SetColor(backgroundColor);

        ExecuteClear();

        SetRadius(brush.Radius);
        brush.RadiusChanged += SetRadius;

        SetColor(brush.Color);
        brush.ColorChanged += SetColor;

        image.texture = texture;
    }

    void SetRadius(float v) => cs.SetFloat("Radius", v);
    void SetColor(Color c) => cs.SetFloats("Color", c.r, c.g, c.b, c.a);

    void Dispatch(int kernelIndex) => cs.Dispatch(kernelIndex, threadCount.x, threadCount.y, 1);

    public void ExecuteClear() => Dispatch(0);

    void ExecuteDrawPoint() => Dispatch(1);
    void ExecuteDrawLine() => Dispatch(2);


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
        ExecuteDrawPoint();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var p = GetBrushPosition(eventData.position);
        DrawContinuousLine(p);
    }

    void DrawContinuousLine(Vector2 next)
    {
        cs.SetFloats("B", next.x, next.y);
        ExecuteDrawLine();
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
