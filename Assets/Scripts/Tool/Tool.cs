using Recounter.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public abstract class Tool<T> : Item, IHoverHandler<T> where T : class
    {
        [SerializeField] LayerMask _toolableMask;
        [SerializeField] LayerMask _allMask;
        [SerializeField] LayerMask _dropMask;
        [SerializeField] float _range;
        [SerializeField] Rigidbody _rb;
        [SerializeField] BoxCollider _collider;

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

        void OnThrow(InputAction.CallbackContext obj)
        {
            var toItem = transform.position - s_camera.transform.position;
            if (Physics.CheckBox(transform.TransformPoint(_collider.center), _collider.size / 2, transform.rotation, _dropMask)
                || Physics.Raycast(s_camera.transform.position, toItem, toItem.magnitude, _dropMask))
                return;

            Release();
        }

        void Update()
        {
            if (!IsHeld || IsInteractionInProgress) return;

            _hoverRaycaster.Raycast();
        }

        protected override void OnPickUp()
        {
            InputLayer.Placement.Throw.performed += OnThrow;

            if (!_rb) return;
            _rb.isKinematic = true;
        }

        protected override void OnRelease()
        {
            InputLayer.Placement.Throw.performed -= OnThrow;

            if (!_rb) return;
            _rb.isKinematic = false;
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
