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
        [SerializeField] float _sensitivity;

        [Header("Input")]
        [SerializeField] GraphicRaycaster _raycaster;
        [SerializeField] InputActionReference _lookAction;
        [SerializeField] InputAction _click;

        bool _inUse;
        Camera _camera;

        GameObject _hover;
        GameObject _pressTarget;

        PointerEventData _pointerData;

        List<RaycastResult> _results = new();

        void Awake()
        {
            _click.started += Click;
            _click.canceled += Click;

            _click.Enable();
        }

        void Click(InputAction.CallbackContext ctx)
        {
            var press = ctx.ReadValueAsButton();

            if (press)
            {
                if (!_hover) return;

                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.pointerDownHandler);
                _pressTarget = _hover;
            }
            else if (_pressTarget)
            {
                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerUpHandler);

                if (_pressTarget != _hover) return;

                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerClickHandler);
            }
        }

        void Start()
        {
            _lookAction.action.performed += MoveCursor;
            _camera = Camera.main;

            _pointerData = new PointerEventData(EventSystem.current);
        }

        void MoveCursor(InputAction.CallbackContext ctx)
        {
            _cursor.Translate(ctx.ReadValue<Vector2>() * _sensitivity);

            _pointerData.position = _camera.WorldToScreenPoint(_cursor.position);

            EvaluateCursorEvents();
        }

        protected override void OnInteract(Employee e)
        {
            _inUse = !_inUse;

            e.Controller.Suspend(_inUse);

            if (_inUse)
                _lookAction.action.Enable();
            else
                _lookAction.action.Disable();
        }

        GameObject RaycastUI()
        {
            _results.Clear();
            _raycaster.Raycast(_pointerData, _results);

            var newHover = _results.Count > 0 ? _results[0].gameObject : null;

            var selectable = newHover.GetComponentInParent<Selectable>();

            if (selectable)
            {
                newHover = selectable.gameObject;
            }

            return newHover;
        }

        void EvaluateCursorEvents()
        {
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
