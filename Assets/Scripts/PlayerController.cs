using Cinemachine;
using FMODUnity;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    void PlaySound(EventReference eventRef) => RuntimeManager.PlayOneShot(eventRef, body.position);

    void Update()
    {
        if (Pause.IsPaused) return;

        HandleJump();

        ApplyGravity();

        HandleMovement();

        HandleLooking();
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
        if (controller.isGrounded) return;

        var holdingJump = CanJump && playerControls.Jump.IsPressed();

        var velocityChange = gravity * Time.deltaTime;

        if (yVelocity > 0 && !holdingJump)
            velocityChange *= lowJumpMultiplier;

        yVelocity -= velocityChange;
    }

    void HandleMovement()
    {
        var input = playerControls.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        var isSprinting = playerControls.Sprint.IsPressed() && input.y > 0;

        HandleBobbingAndFootsteps(input, isSprinting);

        AnimateFov(isSprinting);

        if (!CanMove) return;

        var speed = isSprinting ? sprintSpeed : walkSpeed;

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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        fov = walkFov;

        playerControls = new Controls().Player;

        playerControls.Enable();

        RecordCameraAngles();
    }

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
