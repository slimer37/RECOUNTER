using System;
using UnityEngine;

namespace Recounter
{
    public enum GetComponentType { InParent, Self, InChildren }

    public interface IHoverHandler<T> where T : class
    {
        public void HoverEnter(T obj);
        public void HoverStay(T obj);
        public void HoverExit(T obj);
    }

    public class HoverRaycaster<T> where T : class
    {
        readonly Camera _camera;
        readonly float _range;
        readonly LayerMask _raycastMask;
        readonly LayerMask _interactableMask;

        IHoverHandler<T> _handler;

        readonly Func<Transform, T> _getComponent;

        T _hoverTarget;
        Transform _lastHoverTarget;

        public T HoverTarget => _hoverTarget;

        public QueryTriggerInteraction TriggerInteraction { get; set; } = QueryTriggerInteraction.UseGlobal;

        public HoverRaycaster(Camera camera, float range, LayerMask raycastMask, LayerMask interactableMask, GetComponentType getComponentType)
        {
            _camera = camera;
            _range = range;
            _raycastMask = raycastMask;
            _interactableMask = interactableMask;

            _getComponent = getComponentType switch
            {
                GetComponentType.InParent => t => t.GetComponentInParent<T>(),
                GetComponentType.Self => t => t.GetComponent<T>(),
                GetComponentType.InChildren => t => t.GetComponentInChildren<T>(),
                _ => throw new ArgumentOutOfRangeException(nameof(getComponentType))
            };
        }

        public void AssignCallbacks(IHoverHandler<T> handler)
        {
            _handler = handler;
        }

        public void Raycast()
        {
            Transform currentHover = null;

            if (Physics.Raycast(_camera.ViewportPointToRay(Vector2.one / 2), out var hit, _range, _raycastMask, TriggerInteraction))
            {
                if (_interactableMask == (_interactableMask | (1 << hit.transform.gameObject.layer)))
                {
                    currentHover = hit.collider.transform;
                }
            }

            HandleHoverTarget(currentHover);

            if (_hoverTarget != null)
            {
                _handler.HoverStay(_hoverTarget);
            }
        }

        public void Clear()
        {
            HandleHoverTarget(null);
        }

        void HandleHoverTarget(Transform currentHover)
        {
            if (_lastHoverTarget == currentHover) return;

            _lastHoverTarget = currentHover;

            if (_hoverTarget != null)
            {
                _handler.HoverExit(_hoverTarget);
            }

            if (currentHover)
            {
                _hoverTarget = _getComponent(currentHover);

                if (_hoverTarget == null)
                {
                    Debug.LogError($"Couldn't find a component of type \"{typeof(T).Name}\" for the hovered object.");
                    return;
                }

                _handler.HoverEnter(_hoverTarget);
            }
            else
            {
                _hoverTarget = null;
            }
        }
    }
}
