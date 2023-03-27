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

        List<RaycastResult> _results = new();

        void Awake()
        {
            _click.started += Click;
            _click.canceled += Click;

            _click.Enable();
        }

        void Click(InputAction.CallbackContext ctx)
        {
            var pointer = new PointerEventData(EventSystem.current);

            var press = ctx.ReadValueAsButton();

            if (press)
            {
                if (!_hover) return;

                ExecuteEvents.Execute(_hover, pointer, ExecuteEvents.pointerDownHandler);
                _pressTarget = _hover;
            }
            else if (_pressTarget)
            {
                ExecuteEvents.Execute(_pressTarget, pointer, ExecuteEvents.pointerUpHandler);

                if (_pressTarget != _hover) return;

                ExecuteEvents.Execute(_pressTarget, pointer, ExecuteEvents.pointerClickHandler);
            }
        }

        void Start()
        {
            _lookAction.action.performed += MoveCursor;
            _camera = Camera.main;
        }

        void MoveCursor(InputAction.CallbackContext ctx)
        {
            _cursor.Translate(ctx.ReadValue<Vector2>() * _sensitivity);
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

        void Update()
        {
            if (!_inUse) return;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = _camera.WorldToScreenPoint(_cursor.position)
            };

            _results.Clear();
            _raycaster.Raycast(pointerData, _results);

            var newHover = _results.Count > 0 ? _results[0].gameObject : null;

            if (_hover && _hover != newHover)
            {
                ExecuteEvents.Execute(_hover, pointerData, ExecuteEvents.pointerExitHandler);
            }

            _hover = newHover;

            if (_hover)
            {
                ExecuteEvents.Execute(_hover, pointerData, ExecuteEvents.pointerEnterHandler);
            }
        }
    }
}
