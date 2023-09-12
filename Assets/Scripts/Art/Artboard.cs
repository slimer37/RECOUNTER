using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter.Art
{
    public class Artboard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Texture")]
        [SerializeField, Min(1)] Vector2Int resolution = Vector2Int.one * 100;
        [SerializeField] RawImage image;
        [SerializeField] FilterMode mode;

        [Header("Brush")]
        [SerializeField] ComputeShaderBrush defaultBrush;

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

        public Painting Painting { get; private set; }

        ConstrainedUndoRedo<Painting> undoRedo;

        Vector2Int threadCount;
        int clearKernel;

        bool isDrawing;

        public Vector2Int Resolution => resolution;
        public ITool CurrentTool { get; private set; }
        public IBrush CurrentBrush { get; private set; }

        public event Action<IBrush> BrushSelected;
        public event Action<ITool> ToolSelected;

        public void Complete()
        {
            ArtCreator.Complete(Painting.Texture);
        }

        void Awake()
        {
            threadCount = new(resolution.x / 8, resolution.y / 8);
            clearKernel = clearCs.FindKernel("Clear");

            var texture = new RenderTexture(resolution.x, resolution.y, 0)
            {
                enableRandomWrite = true,
                filterMode = mode
            };

            Painting = new Painting(texture, backgroundColor);

            clearCs.SetTexture(0, "Result", texture);

            SetColor(backgroundColor);

            var session = ArtCreator.CurrentArtSession;
            if (session != null && session.Initial)
            {
                Graphics.CopyTexture(session.Initial, texture);
                Painting.IsClear = false;
            }
            else
            {
                clearCs.Dispatch(clearKernel, threadCount.x, threadCount.y, 1);
            }

            image.texture = texture;

            SetBrush(defaultBrush);

            InitializeUndo();

            clearButton.onClick.AddListener(ClearBoard);

            UpdateButtons();
        }

        void OnDisable()
        {
            undo.Disable();
            redo.Disable();
        }

        void OnEnable()
        {
            undo.Enable();
            redo.Enable();
        }

        public void SetTool(ITool newTool)
        {
            CurrentTool?.Deactivate();

            CurrentTool = newTool;
            ToolSelected?.Invoke(newTool);
            
            CurrentTool.Activate(Painting.Texture);

            CurrentBrush = null;

            if (CurrentTool is IBrush brush)
            {
                CurrentBrush = brush;
                BrushSelected?.Invoke(brush);
            }
        }

        public void SetBrush(ComputeShaderBrush newBrush) => SetTool(newBrush);
        public void SetTool(ToolBehaviour tool) => SetTool(tool as ITool);

        void SetColor(Color c) => clearCs.SetFloats("Color", c.r, c.g, c.b, c.a);

        public void ClearBoard()
        {
            if (Painting.IsClear) return;

            Painting.IsClear = true;

            clearCs.Dispatch(clearKernel, threadCount.x, threadCount.y, 1);

            RecordDraw();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Painting.IsClear = false;
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

        void Draw(Vector2 point) => CurrentTool.Draw(point.x, point.y);

        public void OnDrag(PointerEventData eventData)
        {
            var p = GetBrushPosition(eventData.position);
            DrawContinuousLine(p);
        }

        void DrawContinuousLine(Vector2 next) => CurrentBrush?.DrawContinuousLine(next.x, next.y);

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
                undoTextures[i] = new Painting(texture, backgroundColor);
            }

            undoRedo = new ConstrainedUndoRedo<Painting>(undoLimit, undoTextures, Painting, Painting.Restore);

            undo.performed += Undo;
            redo.performed += Redo;

            undoButton.onClick.AddListener(Undo);
            redoButton.onClick.AddListener(Redo);
        }

        void UpdateButtons()
        {
            undoButton.interactable = undoRedo.CanUndo;
            redoButton.interactable = undoRedo.CanRedo;
            clearButton.interactable = !Painting.IsClear;
        }

        void RecordDraw()
        {
            undoRedo.RecordState(Painting);
            UpdateButtons();
        }

        public void Undo()
        {
            if (isDrawing) return;

            undoRedo.Undo(Painting, out _);
            UpdateButtons();
        }

        public void Redo()
        {
            if (isDrawing) return;

            undoRedo.Redo(Painting, out _);
            UpdateButtons();
        }

        void Undo(InputAction.CallbackContext ctx) => Undo();
        void Redo(InputAction.CallbackContext ctx) => Redo();
    }
}
