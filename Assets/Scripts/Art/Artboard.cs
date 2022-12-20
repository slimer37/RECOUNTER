using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Texture")]
    [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
    [SerializeField] RawImage image;
    [SerializeField] FilterMode mode;

    [Header("Brush")]
    [SerializeField] Brush brush;

    [Header("Undo/Redo")]
    [SerializeField] InputAction undo;
    [SerializeField] InputAction redo;
    [SerializeField] Button redoButton;
    [SerializeField] Button undoButton;
    [SerializeField] int undoLimit;

    [Header("Clearing")]
    [SerializeField] Color backgroundColor;
    [SerializeField] ComputeShader clearCs;
    [SerializeField] Button clearButton;

    Painting painting;

    ConstrainedUndoRedo<Painting> undoRedo;

    Vector2Int threadCount;
    int clearKernel;

    bool isDrawing;

    void Awake()
    {
        threadCount = new(resolution.x / 8, resolution.y / 8);
        clearKernel = clearCs.FindKernel("Clear");

        var texture = new RenderTexture(resolution.x, resolution.y, 0)
        {
            enableRandomWrite = true,
            filterMode = mode
        };

        painting = new Painting(texture);

        clearCs.SetTexture(0, "Result", texture);

        SetColor(backgroundColor);
        clearCs.Dispatch(clearKernel, threadCount.x, threadCount.y, 1);

        image.texture = texture;

        SetBrush(brush);

        InitializeUndo();

        clearButton.onClick.AddListener(ClearBoard);

        UpdateButtons();
    }

    public void SetBrush(Brush newBrush)
    {
        brush = newBrush;
        brush.InitializeWithTexture(painting.Texture);
    }

    void SetColor(Color c) => clearCs.SetFloats("Color", c.r, c.g, c.b, c.a);

    public void ClearBoard()
    {
        if (painting.IsClear) return;

        painting.IsClear = true;

        clearCs.Dispatch(clearKernel, threadCount.x, threadCount.y, 1);

        RecordDraw();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        painting.IsClear = false;
        isDrawing = true;

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

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
        RecordDraw();
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

    void InitializeUndo()
    {
        var undoTextures = new Painting[undoLimit];

        for (int i = 0; i < undoTextures.Length; i++)
        {
            var texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
            undoTextures[i] = new Painting(texture);
        }

        undoRedo = new ConstrainedUndoRedo<Painting>(undoLimit, undoTextures, painting, Painting.Restore);

        undo.performed += Undo;
        undo.Enable();

        redo.performed += Redo;
        redo.Enable();

        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
    }

    void UpdateButtons()
    {
        undoButton.interactable = undoRedo.CanUndo;
        redoButton.interactable = undoRedo.CanRedo;
        clearButton.interactable = !painting.IsClear;
    }

    void RecordDraw()
    {
        undoRedo.RecordState(painting);
        UpdateButtons();
    }

    public void Undo()
    {
        if (isDrawing) return;

        undoRedo.Undo(painting, out _);
        UpdateButtons();
    }

    public void Redo()
    {
        if (isDrawing) return;

        undoRedo.Redo(painting, out _);
        UpdateButtons();
    }

    void Undo(InputAction.CallbackContext ctx) => Undo();
    void Redo(InputAction.CallbackContext ctx) => Redo();
}
