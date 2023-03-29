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

        void Start()
        {
            _camera = Camera.main;

            _pointerData = new PointerEventData(EventSystem.current);
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

        protected override void OnInteract(Employee e)
        {
            _inUse = !_inUse;

            InputLayer.SuspendMovement(_inUse);

            e.ShowHud(!_inUse);
        }

        GameObject RaycastUI()
        {
            _results.Clear();
            _raycaster.Raycast(_pointerData, _results);

            if (_results.Count == 0) return null;

            var newHover = _results[0].gameObject;

            var selectable = newHover.GetComponentInParent<Selectable>();

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

            var newHover = RaycastUI();

            if (_hover && _hover != newHover)
            {
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerExitHandler);
            }

            _hover = newHover;

            if (_hover)
            {
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerEnterHandler);
            }
        }
    }
}
