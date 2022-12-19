using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Texture")]
    [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
    [SerializeField] Color backgroundColor;
    [SerializeField] ComputeShader clearCs;
    [SerializeField] RawImage image;
    [SerializeField] FilterMode mode;

    [Header("Brush")]
    [SerializeField] Brush brush;

    [Header("Undo")]
    [SerializeField] InputAction undo;
    [SerializeField] int undoLimit;

    RenderTexture texture;

    Texture2D[] undoTextures;
    int currentUndoStep;

    DropOutStack<int> undoStack;

    Vector2Int threadCount;
    int clearKernel;

    bool isDrawing;

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

        InitializeUndo();
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
        isDrawing = true;

        RecordUndo();

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

    public void OnPointerUp(PointerEventData eventData) => isDrawing = false;

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

    void InitializeUndo()
    {
        undoTextures = new Texture2D[undoLimit];
        undoStack = new DropOutStack<int>(undoLimit);

        for (int i = 0; i < undoTextures.Length; i++)
        {
            undoTextures[i] = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
        }

        undo.performed += Undo;
        undo.Enable();
    }

    void RecordUndo()
    {
        currentUndoStep %= undoTextures.Length;

        Graphics.CopyTexture(texture, undoTextures[currentUndoStep]);

        undoStack.Push(currentUndoStep);

        currentUndoStep++;
    }

    void Undo()
    {
        if (isDrawing || !undoStack.TryPop(out var step)) return;

        Graphics.CopyTexture(undoTextures[step], texture);
    }

    void Undo(InputAction.CallbackContext ctx) => Undo();
}
