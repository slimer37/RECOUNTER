using NaughtyAttributes;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] float _pullPointDistance;
    [SerializeField] float _pullStrength;
    [SerializeField, Required] Transform _rotator;
    [SerializeField] Vector3 _pullDirection = Vector3.forward;
    [SerializeField] Vector3 _rotationAxis = Vector3.up;
    [SerializeField] float _deceleration;
    [SerializeField, MinMaxSlider(-180, 180)] Vector2 _limits = new(-180, 180);

    float _rotation;
    float _rotationVelocity;

    protected override bool CanInteract(Employee e) => !IsInteractionInProgress;

    public override HudInfo GetHudInfo(Employee e) => CanInteract(e)
        ? new HudInfo { icon = Icon.Door, text = "Push or Pull" }
        : BlankHud;

    [Button("Normalize Axes")]
    void NormalizeAxis()
    {
        _rotationAxis.Normalize();
        _pullDirection.Normalize();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_rotator.position, _rotationAxis);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_rotator.position, _rotator.TransformDirection(_pullDirection));
    }

    void Awake()
    {
        _rotation = Vector3.Dot(_rotator.eulerAngles, _rotationAxis);
    }

    void Update()
    {
        if (IsInteractionInProgress)
        {
            var pullPoint = Interactor.transform.TransformPoint(Vector3.forward * _pullPointDistance);
            var pullDir = _rotator.TransformDirection(_pullDirection);
            var pull = Vector3.Dot(pullPoint - _rotator.position, pullDir);

            _rotationVelocity = pull * _pullStrength * Time.deltaTime;
        }

        _rotationVelocity = Mathf.MoveTowards(_rotationVelocity, 0, _deceleration * Time.deltaTime);

        _rotation += _rotationVelocity;

        _rotation = Mathf.Clamp(_rotation, _limits.x, _limits.y);

        _rotator.eulerAngles = _rotationAxis * _rotation;
    }
}
