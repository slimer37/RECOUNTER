using NaughtyAttributes;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] float _pullStrength;
    [SerializeField, Required] Transform _rotator;
    [SerializeField] Vector3 _pullDirection = Vector3.forward;
    [SerializeField, Min(0)] float _closeAngle;
    [SerializeField] float _closingForce;

    [Header("Colliders")]
    [SerializeField, Required] Collider _door;
    [SerializeField] Collider _doorFrame;

    HingeJoint _hinge;
    Rigidbody _rb;

    Vector3 _startPullPoint;
    float _pullPointDistance;

    bool _pushingPlayer;

    protected override bool CanInteract(Employee e) => !IsInteractionInProgress;

    protected override HudInfo FormHud(Employee e) => new()
    {
        icon = Icon.Door,
        text = "Push or Pull"
    };

    [Button("Normalize Axes")]
    void NormalizeAxis()
    {
        _pullDirection.Normalize();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_rotator.position, _rotator.TransformDirection(_pullDirection));
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _hinge = GetComponent<HingeJoint>();

        if (_door && _doorFrame)
        {
            Physics.IgnoreCollision(_door, _doorFrame);
        }
    }

    protected override void OnInteract(Employee e)
    {
        var camTransform = e.Camera.transform;
        _door.Raycast(new Ray(camTransform.position, camTransform.forward), out var hit, 100);
        _startPullPoint = transform.InverseTransformPoint(hit.point);
        _hinge.useMotor = false;
        _pullPointDistance = Mathf.Max(1, hit.distance);
    }

    void FixedUpdate()
    {
        if (IsInteractionInProgress)
        {
            var pullPoint = Interactor.Camera.transform.TransformPoint(Vector3.forward * _pullPointDistance);
            var forceTo = transform.InverseTransformPoint(pullPoint);
            var force = forceTo - _startPullPoint;
            var moment = Vector3.Cross(_startPullPoint, force);
            var pull = Vector3.Dot(moment, _hinge.axis) * _pullStrength;

            _rb.angularVelocity = _hinge.axis * pull;
        }
        else if (Mathf.Abs(_hinge.angle) < _closeAngle)
        {
            var rotationVelocity = _closingForce * _closeAngle - Mathf.Abs(_hinge.angle);

            _rb.angularVelocity = _hinge.axis * (rotationVelocity * Mathf.Deg2Rad);
        }
    }

    void Rotate(float speed)
    {
        _hinge.useMotor = true;
        
        var motor = _hinge.motor;
        motor.targetVelocity = speed;
        _hinge.motor = motor;
    }
}
