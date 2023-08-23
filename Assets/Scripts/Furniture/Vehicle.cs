using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public class Vehicle : Interactable
    {
        [SerializeField] Hand.ViewmodelPose _leftHandPose;
        [SerializeField] Hand.ViewmodelPose _rightHandPose;
        [SerializeField] Tool _tool;
        [SerializeField] float _distance;

        [Header("Control")]
        [SerializeField] float _speed;
        [SerializeField] float _turnSpeed;
        [SerializeField] CharacterController _controller;

        bool _isBeingPushed;

        InputAction _movementAction;

        void Awake()
        {
            InputLayer.Placement.Throw.performed += Interact;
        }

        void Interact(InputAction.CallbackContext obj)
        {
            if (!_isBeingPushed) return;

            StopBeingPushed();
        }

        protected override bool CanInteract(Employee e) => !_isBeingPushed && e.HandsAreFree;

        protected override void OnInteract(Employee e) => StartBeingPushed(e);

        void StartBeingPushed(Employee e)
        {
            _isBeingPushed = true;

            e.Interaction.Suspend(true);
            InputLayer.Movement.Move.Disable();

            _movementAction = InputLayer.Movement.Move.Clone();
            _movementAction.Enable();

            e.LeftHand.SetHandViewmodel(_leftHandPose);
            e.RightHand.SetHandViewmodel(_rightHandPose);

            _tool.Equip();

            SetRotation(e);
        }

        void SetRotation(Employee e)
        {
            var forward = transform.forward;
            forward.y = 0;

            var angleY = Quaternion.FromToRotation(Vector3.forward, forward).eulerAngles.y;

            var rot = e.Controller.CameraRotation;

            rot.y = angleY;

            e.Controller.SetCameraRotation(rot);
        }

        void StopBeingPushed()
        {
            _isBeingPushed = false;

            LastInteractor.Interaction.Suspend(false);
            InputLayer.Movement.Move.Enable();

            _movementAction.Dispose();

            LastInteractor.LeftHand.ResetHandViewmodel();
            LastInteractor.RightHand.ResetHandViewmodel();

            _tool.Unequip();
        }

        void Update()
        {
            if (!_isBeingPushed) return;

            var input = _movementAction.ReadValue<Vector2>();

            _controller.SimpleMove(_speed * input.y * transform.forward);

            transform.Rotate(Time.deltaTime * _turnSpeed * input.x * Vector3.up);

            var playerPos = LastInteractor.transform.position;

            var pos = transform.position - transform.forward * _distance;

            pos.y = playerPos.y;

            LastInteractor.transform.position = pos;
        }
    }
}
