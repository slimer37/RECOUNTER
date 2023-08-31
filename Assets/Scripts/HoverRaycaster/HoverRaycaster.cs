using System;
using UnityEngine;

namespace Recounter
{
    public interface IHoverHandler<T> where T : class
    {
        public void HoverEnter(T obj);
        public void HoverStay(T obj);
        public void HoverExit(T obj);
        public void OnRaycastHit(RaycastHit hit) { }
    }

    /// <summary>
    /// Simplified hover raycaster that returns transforms.
    /// </summary>
    public class HoverRaycaster : HoverRaycaster<Transform>
    {
        public HoverRaycaster(Camera camera, float range, LayerMask raycastMask, LayerMask interactableMask, IHoverHandler<Transform> handler)
            : base(camera, range, raycastMask, interactableMask, handler)
        {
            _filter = t => t;
        }
    }

    /// <summary>
    /// Raycaster with hover callbacks that filters via <see cref="GameObject.GetComponent{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to search for with <see cref="GameObject.GetComponent{T}"/></typeparam>
    public class HoverRaycaster<T> where T : class
    {
        readonly Camera _camera;
        readonly float _range;
        readonly LayerMask _raycastMask;
        readonly LayerMask _interactableMask;

        IHoverHandler<T> _handler;

        protected Func<Transform, T> _filter;

        T _hoverTarget;
        Transform _lastHoverTarget;

        public T HoverTarget => _hoverTarget;

        public QueryTriggerInteraction TriggerInteraction { get; set; } = QueryTriggerInteraction.UseGlobal;

        protected HoverRaycaster(Camera camera, float range, LayerMask raycastMask, LayerMask interactableMask, IHoverHandler<T> handler)
        {
            _camera = camera;
            _range = range;
            _raycastMask = raycastMask;
            _interactableMask = interactableMask;

            _handler = handler;
        }

        public HoverRaycaster(Camera camera, float range, LayerMask raycastMask, LayerMask interactableMask, GetComponentType getComponentType, IHoverHandler<T> handler)
            : this(camera, range, raycastMask, interactableMask, handler)
        {
            _filter = getComponentType.GetMethod<T>();
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

                _handler.OnRaycastHit(hit);
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
                _hoverTarget = _filter(currentHover);

                if (_hoverTarget == null) return;

                _handler.HoverEnter(_hoverTarget);
            }
            else
            {
                _hoverTarget = null;
            }
        }
    }
}
