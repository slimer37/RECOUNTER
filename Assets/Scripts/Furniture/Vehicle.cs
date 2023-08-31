using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public class Vehicle : Interactable
    {
        [SerializeField] Hand.ViewmodelPose _leftHandPose;
        [SerializeField] Hand.ViewmodelPose _rightHandPose;
        [SerializeField] float _distance;

        [Header("Control")]
        [SerializeField] protected float _defaultSpeed;
        [SerializeField] protected float _turnSpeed;
        [SerializeField] Collider _driverCollider;
        [SerializeField] Rigidbody _rigidbody;

        protected bool IsBeingPushed { get; private set; }

        InputAction _movementAction;

        Vector2 input;

        protected virtual float Speed => _defaultSpeed;
        protected virtual float TurnSpeed => _turnSpeed;

        protected bool Locked { get; set; }

        protected virtual void Awake()
        {
            InputLayer.Placement.Throw.performed += Interact;
            _driverCollider.enabled = false;
        }

        void Interact(InputAction.CallbackContext obj)
        {
            if (!IsBeingPushed) return;

            StopBeingPushed();
        }

        protected override bool CanInteract(Employee e) => !Locked && !IsBeingPushed && e.HandsAreFree;

        protected override void OnInteract(Employee e) => StartBeingPushed(e);

        void StartBeingPushed(Employee e)
        {
            IsBeingPushed = true;

            e.Interaction.Suspend(true);
            InputLayer.Movement.Move.Disable();

            _movementAction = InputLayer.Movement.Move.Clone();
            _movementAction.Enable();

            e.LeftHand.SetHandViewmodel(_leftHandPose);
            e.RightHand.SetHandViewmodel(_rightHandPose);

            _driverCollider.enabled = true;

            SetRotation(e);

            OnStartedBeingPushed();
        }

        protected virtual void OnStartedBeingPushed() { }

        void SetRotation(Employee e)
        {
            var forward = transform.forward;
            forward.y = 0;

            var angleY = Quaternion.FromToRotation(Vector3.forward, forward).eulerAngles.y;

            var rot = e.Controller.CameraRotation;

            rot.y = angleY;

            e.Controller.CameraRotation = rot;
        }

        void StopBeingPushed()
        {
            IsBeingPushed = false;

            LastInteractor.Interaction.Suspend(false);
            InputLayer.Movement.Move.Enable();

            _movementAction.Dispose();

            LastInteractor.LeftHand.ResetHandViewmodel();
            LastInteractor.RightHand.ResetHandViewmodel();

            _driverCollider.enabled = false;

            OnStoppedBeingPushed();
        }

        protected virtual void OnStoppedBeingPushed() { }

        void FixedUpdate()
        {
            if (!IsBeingPushed || Locked) return;

            if (input.y != 0)
            {
                _rigidbody.AddTorque(Mathf.Deg2Rad * TurnSpeed * input.x * Vector3.up, ForceMode.VelocityChange);
            }
        }

        void Update()
        {
            if (!IsBeingPushed || Locked) return;

            input = _movementAction.ReadValue<Vector2>();

            var forward = transform.forward;

            forward.y = 0;

            var velocity = Speed * input.y * forward;

            velocity.y = _rigidbody.velocity.y;

            _rigidbody.velocity = velocity;

            LastInteractor.Controller.CameraRotation += _rigidbody.angularVelocity.y * Mathf.Rad2Deg * Time.deltaTime * Vector2.up;

            var playerPos = LastInteractor.transform.position;

            var pos = transform.position - transform.forward * _distance;

            pos.y = playerPos.y;

            LastInteractor.transform.position = pos;

            PushingUpdate();
        }

        protected virtual void PushingUpdate() { }
    }
}
