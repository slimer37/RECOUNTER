using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter
{
    public class PhysicalScreen : Interactable
    {
        [SerializeField] RectTransform _cursor;
        [SerializeField] RectTransform _canvas;
        [SerializeField] float _sensitivity;

        [Header("Cursors")]
        [SerializeField] Graphic _linkCursor;
        [SerializeField] Graphic _defaultCursor;

        [Header("Input")]
        [SerializeField] GraphicRaycaster _raycaster;

        bool _inUse;
        bool _mouseDown;

        Camera _camera;

        GameObject _hover;
        GameObject _pressTarget;

        PointerEventData _pointerData;

        List<RaycastResult> _results = new();

        void Awake()
        {
            var click = InputLayer.Menu.Click;

            click.started += Click;
            click.canceled += Click;

            InputLayer.Menu.MoveMouse.performed += MoveCursor;
            InputLayer.Menu.Exit.performed += Exit;

            _linkCursor.enabled = false;
            _defaultCursor.enabled = true;
        }

        void Start()
        {
            _camera = Camera.main;

            _pointerData = new PointerEventData(EventSystem.current);
        }

        void Exit(InputAction.CallbackContext ctx)
        {
            if (!_inUse) return;

            ToggleActive();
        }

        void Click(InputAction.CallbackContext ctx)
        {
            if (!_inUse) return;

            _mouseDown = ctx.ReadValueAsButton();

            if (_mouseDown)
            {
                if (!_hover) return;

                _pointerData.pointerPressRaycast = _pointerData.pointerCurrentRaycast;

                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.beginDragHandler);

                _pressTarget = _hover;
            }
            else if (_pressTarget)
            {
                EvaluateCursorEvents();

                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.endDragHandler);

                if (_pressTarget != _hover) return;

                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerClickHandler);

                _pressTarget = null;
            }
        }

        void MoveCursor(InputAction.CallbackContext ctx)
        {
            if (!_inUse) return;

            _cursor.anchoredPosition += ctx.ReadValue<Vector2>() * _sensitivity;

            _cursor.anchoredPosition = new Vector3(
                Mathf.Clamp(_cursor.anchoredPosition.x, -_canvas.sizeDelta.x / 2, _canvas.sizeDelta.x / 2),
                Mathf.Clamp(_cursor.anchoredPosition.y, -_canvas.sizeDelta.y / 2, _canvas.sizeDelta.y / 2)
                );

            _pointerData.position = _camera.WorldToScreenPoint(_cursor.position);

            EvaluateCursorEvents();
        }

        void ToggleActive()
        {
            _inUse = !_inUse;

            InputLayer.Suspend(_inUse);

            LastInteractor.ShowHud(!_inUse);

            Pause.SetEnabled(!_inUse);
        }

        protected override void OnInteract(Employee e)
        {
            ToggleActive();
        }

        GameObject RaycastUI(out Selectable selectable)
        {
            _results.Clear();
            _raycaster.Raycast(_pointerData, _results);

            selectable = null;

            if (_results.Count == 0) return null;

            var newHover = _results[0].gameObject;

            selectable = newHover.GetComponentInParent<Selectable>();

            if (selectable)
            {
                newHover = selectable.gameObject;
            }

            _pointerData.pointerCurrentRaycast = _results[0];

            return newHover;
        }

        void EvaluateCursorEvents()
        {
            if (_mouseDown && _pressTarget)
            {
                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.dragHandler);
                return;
            }

            var newHover = RaycastUI(out var selectable);

            if (_hover != newHover)
            {
                _linkCursor.enabled = selectable;
                _defaultCursor.enabled = !selectable;

                if (_hover)
                {
                    ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerExitHandler);
                }
            }

            _hover = newHover;

            if (_hover)
            {
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerEnterHandler);
            }
        }
    }
}
