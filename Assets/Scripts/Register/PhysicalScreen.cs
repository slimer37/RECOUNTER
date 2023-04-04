using Cinemachine;
using FMODUnity;
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

        [Header("Camera")]
        [SerializeField] CinemachineVirtualCamera _vcam;
        [SerializeField] RectTransform _follow;
        [SerializeField] float _reduction;

        [Header("Cursors")]
        [SerializeField] Graphic _defaultCursor;
        [SerializeField] Graphic _linkCursor;

        [Header("Input")]
        [SerializeField] GraphicRaycaster _raycaster;

        [Header("Sounds")]
        [SerializeField] EventReference _beepSfx;
        [SerializeField] Toggle _volumeSwitch;

        [Header("Fun Mouse")]
        [SerializeField] PhysicalMouse _mouse;

        bool _inUse;
        bool _mouseDown;

        Camera _camera;

        GameObject _hover;
        Selectable _hoveredSelectable;
        bool _hoverInteractable;

        GameObject _pressTarget;

        PointerEventData _pointerData;

        List<RaycastResult> _results = new();

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = Icon.Access,
            text = "Access"
        };

        void Awake()
        {
            var click = InputLayer.Menu.Click;

            click.started += Click;
            click.canceled += Click;

            InputLayer.Menu.MoveMouse.performed += MoveCursor;
            InputLayer.Menu.Exit.performed += Exit;

            _linkCursor.enabled = false;
            _defaultCursor.enabled = true;

            _raycaster.enabled = false;
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

            DoClick(ctx.ReadValueAsButton());
        }

        void MoveCursor(InputAction.CallbackContext ctx)
        {
            if (!_inUse) return;

            _cursor.anchoredPosition += ctx.ReadValue<Vector2>() * _sensitivity;

            _cursor.anchoredPosition = new Vector3(
                Mathf.Clamp(_cursor.anchoredPosition.x, -_canvas.sizeDelta.x / 2, _canvas.sizeDelta.x / 2),
                Mathf.Clamp(_cursor.anchoredPosition.y, -_canvas.sizeDelta.y / 2, _canvas.sizeDelta.y / 2)
                );

            _follow.anchoredPosition = _cursor.anchoredPosition / _reduction;

            _pointerData.position = _camera.WorldToScreenPoint(_cursor.position);

            EvaluateCursorEvents();

            _mouse.Move(_cursor.anchoredPosition);
        }

        void ToggleActive()
        {
            _inUse = !_inUse;

            InputLayer.Suspend(_inUse);

            LastInteractor.ShowHud(!_inUse);

            Pause.SetEnabled(!_inUse);

            if (_vcam) _vcam.enabled = _inUse;

            if (!_inUse && _mouseDown)
            {
                DoClick(false, true);
            }

            if (_inUse)
            {
                _mouse.StartUsing(LastInteractor.RightHand);
            }
            else
            {
                _mouse.StopUsing();
            }
        }

        protected override void OnInteract(Employee e)
        {
            print("interact by " + Interactor);
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

        void DoClick(bool press, bool denyClick = false)
        {
            _mouseDown = press;

            EvaluateCursorEvents();

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
                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(_hover, _pointerData, ExecuteEvents.endDragHandler);

                if (_pressTarget != _hover || denyClick) return;

                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.pointerClickHandler);

                if (_volumeSwitch.isOn && _hoverInteractable && _hoveredSelectable is Button)
                    RuntimeManager.PlayOneShotAttached(_beepSfx, gameObject);

                _pressTarget = null;
            }

            EvaluateCursorEvents();
        }

        void EvaluateCursorEvents()
        {
            if (_mouseDown && _pressTarget)
            {
                ExecuteEvents.Execute(_pressTarget, _pointerData, ExecuteEvents.dragHandler);
            }

            var newHover = RaycastUI(out _hoveredSelectable);

            if (_hover != newHover)
            {
                _hoverInteractable = _hoveredSelectable && _hoveredSelectable.IsInteractable();
                _linkCursor.enabled = _hoverInteractable;
                _defaultCursor.enabled = !_hoverInteractable;

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
