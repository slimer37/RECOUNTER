using Recounter.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public abstract class Tool<T> : Item, IHoverHandler<T> where T : class
    {
        [SerializeField] LayerMask _toolableMask;
        [SerializeField] LayerMask _allMask;
        [SerializeField] float _range;

        static Camera s_camera;

        HoverRaycaster<T> _hoverRaycaster;

        InputAction _use;

        void Start()
        {
            if (s_camera == null)
                s_camera = Camera.main;

            _hoverRaycaster = new(s_camera, _range, _allMask, _toolableMask, GetComponentType.InParent, this);

            _use = InputLayer.Interaction.Interact;
        }

        void Update()
        {
            if (!IsHeld) return;

            _hoverRaycaster.Raycast();
        }

        public void HoverEnter(T obj) { }

        public void HoverStay(T obj)
        {
            if (_use.WasPressedThisFrame())
            {
                UseOn(obj);
            }
        }

        public void HoverExit(T obj) { }

        protected abstract void UseOn(T obj);
    }
}
