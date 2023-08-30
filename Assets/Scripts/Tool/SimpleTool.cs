using Recounter.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public class SimpleTool : Item
    {
        [SerializeField] LayerMask _dropMask;
        [SerializeField] Rigidbody _rb;
        [SerializeField] BoxCollider _collider;
        [SerializeField] Tool _tool;

        Camera _camera;

        void Start()
        {
            _camera = Camera.main;
        }

        protected override void OnPickUp()
        {
            _tool?.Equip();
            InputLayer.Placement.Throw.performed += OnThrow;

            if (!_rb) return;
            _rb.isKinematic = true;
        }

        protected override void OnRelease()
        {
            _tool?.Unequip();
            InputLayer.Placement.Throw.performed -= OnThrow;

            if (!_rb) return;
            _rb.isKinematic = false;
        }

        void OnThrow(InputAction.CallbackContext obj)
        {
            var toItem = transform.position - _camera.transform.position;
            if (Physics.CheckBox(transform.TransformPoint(_collider.center), _collider.size / 2, transform.rotation, _dropMask)
                || Physics.Raycast(_camera.transform.position, toItem, toItem.magnitude, _dropMask))
                return;

            Release();
        }
    }
}
