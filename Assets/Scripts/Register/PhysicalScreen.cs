using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public class PhysicalScreen : Interactable
    {
        [SerializeField] Transform _cursor;
        [SerializeField] float _sensitivity;
        [SerializeField] InputActionReference _lookAction;

        bool _inUse;

        void Awake()
        {
            _lookAction.action.performed += MoveCursor;
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
    }
}
