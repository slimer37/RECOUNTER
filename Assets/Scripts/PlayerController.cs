using Cinemachine;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

namespace Recounter
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] Animator _animator;
        [SerializeField, AnimatorParam(nameof(_animator))] string _xSpeedParam;
        [SerializeField, AnimatorParam(nameof(_animator))] string _ySpeedParam;
        [SerializeField, AnimatorParam(nameof(_animator))] string _crouchedParam;

        [field: Header("Moving")]
        [field: SerializeField] public bool CanMove { get; set; } = true;
        [SerializeField] float _walkSpeed;
        [SerializeField] float _sprintSpeed;
        [SerializeField] float _inputSmoothing;
        [SerializeField] Transform _body;
        [SerializeField] CharacterController _controller;

        [field: Header("Footsteps/Bobbing")]
        [field: SerializeField] public bool BobbingEnabled { get; set; } = true;
        [SerializeField] CinemachineImpulseSource _walkImpulse;
        [SerializeField] CinemachineImpulseSource _sprintImpulse;

        [field: Header("Jumping")]
        [field: SerializeField] public bool CanJump { get; set; } = true;
        [SerializeField] float _jumpForce;
        [SerializeField] float _gravity;
        [SerializeField] float _lowJumpMultiplier;

        [field: Header("Looking")]
        [field: SerializeField] public bool CanLookAround { get; set; } = true;
        [field: SerializeField] public float Sensitivity { get; set; }
        [SerializeField] Transform _camTarget;
        [SerializeField] float _clamp;

        [field: Header("Crouching")]
        [field: SerializeField] public bool CanCrouch { get; set; } = true;
        [SerializeField] float _crouchedHeight;
        [SerializeField] float _crouchedCamHeight;
        [SerializeField] float _crouchSpeed;
        [SerializeField] float _camHeightSmoothing;
        
        [Header("FOV")]
        [SerializeField] CinemachineVirtualCamera _vcam;
        [SerializeField] float _walkFov;
        [SerializeField] float _sprintFov;
        [SerializeField] float _fovChangeSpeed;
        
        [Header("SFX")]
        [SerializeField] EventReference _jumpSfx;

        Vector2 _camRot;
        float _fov;
        float _yVelocity;

        bool _isSuspended;

        Vector2 _smoothInput;
        Vector2 _smoothInputVelocity;

        float _defaultHeight;
        float _defaultCamHeight;
        float _camHeightVelocity;

        int _xSpeedId;
        int _ySpeedId;

        bool _isSprinting;
        bool _isCrouching;

        Controls.MovementActions _movementInput;

        public bool IsMoving => _controller.velocity.sqrMagnitude > 0;

        public Vector2 CameraRotation
        {
            get => _camRot;
            set => _camRot = value;
        }

        public bool ImpulseFootstep()
        {
            if (_isSuspended || !_controller.isGrounded || !BobbingEnabled) return false;

            var impulse = _isSprinting ? _sprintImpulse : _walkImpulse;

            impulse.GenerateImpulse();

            return true;
        }

        void PlaySound(EventReference eventRef) => RuntimeManager.PlayOneShot(eventRef, _body.position);

        void Update()
        {
            if (Pause.IsPaused) return;

            ApplyGravity();

            HandleJump();

            HandleCrouching();

            HandleMovement();

            HandleLooking();
        }

        void HandleCrouching()
        {
            if (CanCrouch && !_isSuspended)
            {
                _isCrouching = _movementInput.Crouch.IsPressed();
            }

            var height = _isCrouching ? _crouchedHeight : _defaultHeight;
            var goalCamHeight = _isCrouching ? _crouchedCamHeight : _defaultCamHeight;
            var camHeight = Mathf.SmoothDamp(
                _camTarget.localPosition.y,
                goalCamHeight,
                ref _camHeightVelocity,
                _camHeightSmoothing);

            _controller.height = height;
            _controller.center = Vector3.up * height / 2;
            _camTarget.localPosition = Vector3.up * camHeight;

            _animator.SetBool(_crouchedParam, _isCrouching);
        }

        void HandleLooking()
        {
            if (!CanLookAround || _isSuspended) return;

            var look = _movementInput.Look.ReadValue<Vector2>() * Sensitivity;

            _camRot.y += look.x;
            _camRot.x -= look.y;
            _camRot.x = Mathf.Clamp(_camRot.x, -_clamp, _clamp);

            _body.eulerAngles = _camRot.y * Vector3.up;
            _camTarget.localEulerAngles = _camRot.x * Vector3.right;
        }

        void AnimateFov(bool isSprinting)
        {
            var targetFov = isSprinting ? _sprintFov : _walkFov;
            _fov = Mathf.Lerp(_fov, targetFov, _fovChangeSpeed * Time.deltaTime);
            _vcam.m_Lens.FieldOfView = _fov;
        }

        void HandleJump()
        {
            if (!CanJump || !_controller.isGrounded) return;

            if (_movementInput.Jump.WasPressedThisFrame())
            {
                _yVelocity = _jumpForce;
                PlaySound(_jumpSfx);
            }
        }

        void ApplyGravity()
        {
            if (_controller.isGrounded)
            {
                _yVelocity = -_gravity * Time.deltaTime;
                return;
            }

            var velocityChange = _gravity * Time.deltaTime;

            if (CanJump)
            {
                var holdingJump = _movementInput.Jump.IsPressed();

                if (_yVelocity > 0 && !holdingJump)
                    velocityChange *= _lowJumpMultiplier;
            }

            _yVelocity -= velocityChange;
        }

        void HandleMovement()
        {
            var localControllerVelocity = transform.InverseTransformDirection(_controller.velocity);
            _animator.SetFloat(_xSpeedId, localControllerVelocity.x);
            _animator.SetFloat(_ySpeedId, localControllerVelocity.z);

            var input = _movementInput.Move.ReadValue<Vector2>();

            // Sprinting only allowed when moving forward
            _isSprinting = !_isCrouching && _movementInput.Sprint.IsPressed() && input.y > 0;

            AnimateFov(_isSprinting);

            if (!CanMove) return;

            var speed = _isCrouching switch
            {
                true => _crouchSpeed,
                false when _isSprinting => _sprintSpeed,
                false => _walkSpeed
            };

            _smoothInput = Vector2.SmoothDamp(_smoothInput, input * speed, ref _smoothInputVelocity, _inputSmoothing);
            var velocity = _body.TransformDirection(_smoothInput.x, 0, _smoothInput.y) + Vector3.up * _yVelocity;

            _controller.Move(velocity * Time.deltaTime);
        }

        void Awake()
        {
            InputLayer.SetCursor(false);

            _fov = _walkFov;

            _movementInput = InputLayer.Movement;

            RecordCameraAngles();

            _xSpeedId = Animator.StringToHash(_xSpeedParam);
            _ySpeedId = Animator.StringToHash(_ySpeedParam);

            _defaultHeight = _controller.height;
            _defaultCamHeight = _camTarget.localPosition.y;
        }

        void OnDestroy() => InputLayer.SetCursor(true);

        void RecordCameraAngles()
        {
            _camRot.y = _body.eulerAngles.y;
            _camRot.x = _camTarget.localEulerAngles.x;

            if (_camRot.x > 90)
                _camRot.x -= 360;
            else if (_camRot.x < -90)
                _camRot.x += 360;
        }
    }
}
