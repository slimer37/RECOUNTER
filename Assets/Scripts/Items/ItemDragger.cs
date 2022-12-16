using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDragger : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] float _range;
    [SerializeField] float _force;
    [SerializeField] LayerMask _mask;
    [SerializeField, Tag] string _tag;

    [Header("Input")]
    [SerializeField] InputAction _dragAction;

    Vector3 _dragPosition;
    Rigidbody _dragTarget;

    RigidbodyInterpolation _interpolateSetting;
    bool _gravitySetting;

    void Awake()
    {
        _dragAction.performed += StartDrag;
        _dragAction.canceled += EndDrag;
    }

    void OnEnable() => _dragAction.Enable();
    void OnDisable() => _dragAction.Disable();

    void StartDrag(InputAction.CallbackContext ctx)
    {
        if (Physics.Raycast(_camera.ViewportPointToRay(Vector2.one / 2), out var hit, _range, _mask))
        {
            if (!hit.rigidbody || !hit.collider.CompareTag(_tag)) return;

            Setup(hit.rigidbody);
        }
    }

    void FixedUpdate()
    {
        if (!_dragTarget || !Mouse.current.leftButton.isPressed) return;

        var force = _camera.transform.TransformPoint(_dragPosition) - _dragTarget.position;
        var distance = force.magnitude;

        _dragTarget.AddForce(force * _force);

        _dragTarget.velocity *= Mathf.Min(1f, distance / 2);
    }

    void EndDrag(InputAction.CallbackContext ctx)
    {
        if (!_dragTarget) return;

        ClearTarget();
    }

    void Setup(Rigidbody target)
    {
        _dragTarget = target;

        _interpolateSetting = _dragTarget.interpolation;
        _dragTarget.interpolation = RigidbodyInterpolation.Interpolate;

        _gravitySetting = _dragTarget.useGravity;
        _dragTarget.useGravity = false;

        _dragPosition = _camera.transform.InverseTransformPoint(target.position);
    }

    void ClearTarget()
    {
        _dragTarget.interpolation = _interpolateSetting;
        _dragTarget.useGravity = _gravitySetting;

        _dragTarget = null;
    }
}
