using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDragger : MonoBehaviour
{
    [SerializeField] PlayerInteraction interaction;
    [SerializeField] Camera _camera;
    [SerializeField] float _range;
    [SerializeField] float _force;
    [SerializeField] float _maxThrowForce;
    [SerializeField] float _throwMultiplier;
    [SerializeField] LayerMask _mask;
    [SerializeField, Tag] string _tag;

    [Header("UI")]
    [SerializeField] Canvas _instructions;

    [Header("Input")]
    [SerializeField] InputAction _dragAction;

    Vector3 _dragPosition;
    Rigidbody _dragTarget;

    Rigidbody _hoverTarget;

    RigidbodyInterpolation _interpolateSetting;
    bool _gravitySetting;

    bool IsDragging => _dragTarget;

    void Awake()
    {
        _dragAction.performed += StartDrag;
        _dragAction.canceled += EndDrag;
    }

    void OnEnable() => _dragAction.Enable();
    void OnDisable() => _dragAction.Disable();

    void Update()
    {
        if (IsDragging) return;

        _instructions.enabled = false;
        _hoverTarget = null;

        if (Physics.Raycast(_camera.ViewportPointToRay(Vector2.one / 2), out var hit, _range, _mask))
        {
            if (!hit.rigidbody || !hit.collider.CompareTag(_tag)) return;

            _hoverTarget = hit.rigidbody;
            _instructions.enabled = true;
        }
    }

    void StartDrag(InputAction.CallbackContext ctx)
    {
        if (!_hoverTarget) return;

        Setup(_hoverTarget);
        interaction.enabled = false;
        interaction.FadeReticle(true);
    }

    void FixedUpdate()
    {
        if (!_dragTarget || !Mouse.current.leftButton.isPressed) return;

        var force = _camera.transform.TransformPoint(_dragPosition) - _dragTarget.position;
        var distance = force.magnitude;

        _dragTarget.AddForce(force * _force, ForceMode.Acceleration);

        _dragTarget.velocity *= Mathf.Min(1f, distance / 2);
    }

    void EndDrag(InputAction.CallbackContext ctx)
    {
        if (!_dragTarget) return;

        ClearTarget();
        interaction.enabled = true;
    }

    void Setup(Rigidbody target)
    {
        _dragTarget = target;
        _dragTarget.constraints = RigidbodyConstraints.FreezeRotation;

        _interpolateSetting = _dragTarget.interpolation;
        _dragTarget.interpolation = RigidbodyInterpolation.Interpolate;

        _gravitySetting = _dragTarget.useGravity;
        _dragTarget.useGravity = false;

        _dragPosition = _camera.transform.InverseTransformPoint(target.position);
    }

    void ClearTarget()
    {
        _dragTarget.constraints = RigidbodyConstraints.None;
        _dragTarget.interpolation = _interpolateSetting;
        _dragTarget.useGravity = _gravitySetting;

        var magnitude = _dragTarget.velocity.magnitude;
        var throwForce = magnitude * _throwMultiplier;
        _dragTarget.velocity = _dragTarget.velocity / magnitude * Mathf.Min(_maxThrowForce, throwForce);

        _dragTarget = null;
    }
}
