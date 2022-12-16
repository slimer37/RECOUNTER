using NaughtyAttributes;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] float _pullPointDistance;
    [SerializeField] float _pullStrength;
    [SerializeField, Required] Transform _rotator;
    [SerializeField] Vector3 _pullDirection = Vector3.forward;
    [SerializeField] float _deceleration;
    [SerializeField, Min(0)] float _closeAngle;
    [SerializeField] float _closingForce;

    HingeJoint _hinge;

    protected override bool CanInteract(Employee e) => !IsInteractionInProgress;

    public override HudInfo GetHudInfo(Employee e) => CanInteract(e)
        ? new HudInfo { icon = Icon.Door, text = "Push or Pull" }
        : BlankHud;

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
        _hinge = GetComponent<HingeJoint>();
    }

    void Update()
    {
        var rotationVelocity = 0f;

        if (IsInteractionInProgress)
        {
            var pullPoint = Interactor.transform.TransformPoint(Vector3.forward * _pullPointDistance);
            var pullDir = _rotator.TransformDirection(_pullDirection);
            var pull = Vector3.Dot(pullPoint - _rotator.position, pullDir);

            rotationVelocity = pull * _pullStrength;
        }
        else if (Mathf.Abs(_hinge.angle) < _closeAngle)
        {
            rotationVelocity = _closingForce * (_closeAngle - Mathf.Abs(_hinge.angle));
        }

        Rotate(rotationVelocity);
    }

    void Rotate(float speed)
    {
        var motor = _hinge.motor;
        motor.targetVelocity = speed;
        _hinge.motor = motor;
    }
}
