using Cinemachine;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField, AnimatorParam(nameof(animator))] string speedParam;
    [SerializeField, AnimatorParam(nameof(animator))] string crouchedParam;

    [field: Header("Moving")]
    [field: SerializeField] public bool CanMove { get; set; } = true;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float inputSmoothing;
    [SerializeField] Transform body;
    [SerializeField] CharacterController controller;

    [field: Header("Footsteps/Bobbing")]
    [field: SerializeField] public bool BobbingEnabled { get; set; } = true;
    [SerializeField] CinemachineImpulseSource walkImpulse;
    [SerializeField] float walkImpulseInterval;
    [SerializeField] CinemachineImpulseSource sprintImpulse;
    [SerializeField] float sprintImpulseInterval;

    [field: Header("Jumping")]
    [field: SerializeField] public bool CanJump { get; set; } = true;
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] float lowJumpMultiplier;

    [field: Header("Looking")]
    [field: SerializeField] public bool CanLookAround { get; set; } = true;
    [field: SerializeField] public float Sensitivity { get; set; }
    [SerializeField] Transform camTarget;
    [SerializeField] float clamp;

    [field: Header("Crouching")]
    [field: SerializeField] public bool CanCrouch { get; set; } = true;
    [SerializeField] float crouchedHeight;
    [SerializeField] float crouchedCamHeight;
    [SerializeField] float crouchSpeed;

    [Header("FOV")]
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] float walkFov;
    [SerializeField] float sprintFov;
    [SerializeField] float fovChangeSpeed;

    [Header("SFX")]
    [SerializeField] EventReference footstepSfx;
    [SerializeField] EventReference jumpSfx;

    Controls.PlayerActions playerControls;

    Vector2 camRot;
    float fov;
    float yVelocity;
    float bobTime;

    bool isSuspended;

    Vector2 smoothInput;
    Vector2 smoothInputVelocity;

    float defaultHeight;
    float defaultCamHeight;

    int speedId;

    void PlaySound(EventReference eventRef) => RuntimeManager.PlayOneShot(eventRef, body.position);

    void Update()
    {
        if (Pause.IsPaused) return;

        ApplyGravity();

        HandleJump();

        HandleMovement();

        HandleLooking();

        HandleCrouching();
    }

    void HandleCrouching()
    {
        if (!CanCrouch) return;

        var isCrouching = playerControls.Crouch.IsPressed();

        var height = isCrouching ? crouchedHeight : defaultHeight;
        var camHeight = isCrouching ? crouchedCamHeight : defaultCamHeight;

        controller.height = height;
        controller.center = Vector3.up * height / 2;
        camTarget.localPosition = Vector3.up * camHeight;

        animator.SetBool(crouchedParam, isCrouching);
    }

    void HandleLooking()
    {
        if (!CanLookAround || isSuspended) return;

        var look = playerControls.Look.ReadValue<Vector2>() * Sensitivity;

        camRot.y += look.x;
        camRot.x -= look.y;
        camRot.x = Mathf.Clamp(camRot.x, -clamp, clamp);

        body.eulerAngles = camRot.y * Vector3.up;
        camTarget.localEulerAngles = camRot.x * Vector3.right;
    }

    void AnimateFov(bool isSprinting)
    {
        var targetFov = isSprinting ? sprintFov : walkFov;
        fov = Mathf.Lerp(fov, targetFov, fovChangeSpeed * Time.deltaTime);
        vcam.m_Lens.FieldOfView = fov;
    }

    void HandleJump()
    {
        if (!CanJump || !controller.isGrounded) return;

        if (playerControls.Jump.WasPressedThisFrame())
        {
            yVelocity = jumpForce;
            PlaySound(jumpSfx);
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            yVelocity = -gravity * Time.deltaTime;
            return;
        }

        var velocityChange = gravity * Time.deltaTime;

        if (CanJump)
        {
            var holdingJump = playerControls.Jump.IsPressed();

            if (yVelocity > 0 && !holdingJump)
                velocityChange *= lowJumpMultiplier;
        }

        yVelocity -= velocityChange;
    }

    void HandleMovement()
    {
        animator.SetFloat(speedId, controller.velocity.magnitude);

        var input = playerControls.Move.ReadValue<Vector2>();

        var isCrouching = playerControls.Crouch.IsPressed();

        // Sprinting only allowed when moving forward
        var isSprinting = !isCrouching && playerControls.Sprint.IsPressed() && input.y > 0;

        HandleBobbingAndFootsteps(input, isSprinting);

        AnimateFov(isSprinting);

        if (!CanMove) return;

        var speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        smoothInput = Vector2.SmoothDamp(smoothInput, input * speed, ref smoothInputVelocity, inputSmoothing);
        var velocity = body.TransformDirection(smoothInput.x, 0, smoothInput.y) + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleBobbingAndFootsteps(Vector2 movement, bool isSprinting)
    {
        if (!controller.isGrounded) return;

        if (movement.sqrMagnitude > 0)
        {
            var impulse = isSprinting ? sprintImpulse : walkImpulse;

            if (BobbingEnabled && bobTime == 0)
                impulse.GenerateImpulse();

            bobTime += Time.deltaTime;

            var impulseInterval = impulse.m_ImpulseDefinition.m_ImpulseDuration;
            impulseInterval += isSprinting ? sprintImpulseInterval : walkImpulseInterval;

            if (bobTime > impulseInterval)
            {
                if (BobbingEnabled)
                    impulse.GenerateImpulse();

                PlaySound(footstepSfx);
                bobTime = 0;
            }
        }
        else
        {
            bobTime = 0;
        }
    }

    void Awake()
    {
        SetCursor(false);

        fov = walkFov;

        playerControls = new Controls().Player;

        playerControls.Enable();

        RecordCameraAngles();

        speedId = Animator.StringToHash(speedParam);

        defaultHeight = controller.height;
        defaultCamHeight = camTarget.localPosition.y;
    }

    void SetCursor(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void OnDestroy() => SetCursor(true);

    void RecordCameraAngles()
    {
        camRot.y = body.eulerAngles.y;
        camRot.x = camTarget.localEulerAngles.x;

        if (camRot.x > 90)
            camRot.x -= 360;
        else if (camRot.x < -90)
            camRot.x += 360;
    }

    void OnResume()
    {
        playerControls.Enable();

        RecordCameraAngles();
    }

    void OnSuspend() => playerControls.Disable();

    public void Suspend(bool suspend)
    {
        isSuspended = suspend;

        if (suspend)
            OnSuspend();
        else
            OnResume();
    }
}
