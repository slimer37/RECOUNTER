using Cinemachine;
using FMODUnity;
using NaughtyAttributes;
using Recounter;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField, AnimatorParam(nameof(animator))] string xSpeedParam;
    [SerializeField, AnimatorParam(nameof(animator))] string ySpeedParam;
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
    [SerializeField] CinemachineImpulseSource sprintImpulse;

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
    [SerializeField] float camHeightSmoothing;

    [Header("FOV")]
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] float walkFov;
    [SerializeField] float sprintFov;
    [SerializeField] float fovChangeSpeed;

    [Header("SFX")]
    [SerializeField] EventReference jumpSfx;

    Vector2 camRot;
    float fov;
    float yVelocity;

    bool isSuspended;

    Vector2 smoothInput;
    Vector2 smoothInputVelocity;

    float defaultHeight;
    float defaultCamHeight;
    float camHeightVelocity;

    int xSpeedId;
    int ySpeedId;

    bool isSprinting;
    bool isCrouching;

    Controls.MovementActions movementInput;

    public bool IsMoving => controller.velocity.sqrMagnitude > 0;

    public Vector3 CameraRotation => camRot;

    public bool ImpulseFootstep()
    {
        if (isSuspended || !controller.isGrounded || !BobbingEnabled) return false;

        var impulse = isSprinting ? sprintImpulse : walkImpulse;

        impulse.GenerateImpulse();

        return true;
    }

    void PlaySound(EventReference eventRef) => RuntimeManager.PlayOneShot(eventRef, body.position);

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
        if (CanCrouch && !isSuspended)
        {
            isCrouching = movementInput.Crouch.IsPressed();
        }

        var height = isCrouching ? crouchedHeight : defaultHeight;
        var goalCamHeight = isCrouching ? crouchedCamHeight : defaultCamHeight;
        var camHeight = Mathf.SmoothDamp(
            camTarget.localPosition.y,
            goalCamHeight,
            ref camHeightVelocity,
            camHeightSmoothing);

        controller.height = height;
        controller.center = Vector3.up * height / 2;
        camTarget.localPosition = Vector3.up * camHeight;

        animator.SetBool(crouchedParam, isCrouching);
    }

    public void SetCameraRotation(Vector2 euler)
    {
        camRot.x = euler.x;
        camRot.y = euler.y;
    }

    void HandleLooking()
    {
        if (!CanLookAround || isSuspended) return;

        var look = movementInput.Look.ReadValue<Vector2>() * Sensitivity;

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

        if (movementInput.Jump.WasPressedThisFrame())
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
            var holdingJump = movementInput.Jump.IsPressed();

            if (yVelocity > 0 && !holdingJump)
                velocityChange *= lowJumpMultiplier;
        }

        yVelocity -= velocityChange;
    }

    void HandleMovement()
    {
        var localControllerVelocity = transform.InverseTransformDirection(controller.velocity);
        animator.SetFloat(xSpeedId, localControllerVelocity.x);
        animator.SetFloat(ySpeedId, localControllerVelocity.z);

        var input = movementInput.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        isSprinting = !isCrouching && movementInput.Sprint.IsPressed() && input.y > 0;

        AnimateFov(isSprinting);

        if (!CanMove) return;

        var speed = isCrouching switch
        {
            true => crouchSpeed,
            false when isSprinting => sprintSpeed,
            false => walkSpeed
        };

        smoothInput = Vector2.SmoothDamp(smoothInput, input * speed, ref smoothInputVelocity, inputSmoothing);
        var velocity = body.TransformDirection(smoothInput.x, 0, smoothInput.y) + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void Awake()
    {
        InputLayer.SetCursor(false);

        fov = walkFov;

        movementInput = InputLayer.Movement;

        RecordCameraAngles();

        xSpeedId = Animator.StringToHash(xSpeedParam);
        ySpeedId = Animator.StringToHash(ySpeedParam);

        defaultHeight = controller.height;
        defaultCamHeight = camTarget.localPosition.y;
    }

    void OnDestroy() => InputLayer.SetCursor(true);

    void RecordCameraAngles()
    {
        camRot.y = body.eulerAngles.y;
        camRot.x = camTarget.localEulerAngles.x;

        if (camRot.x > 90)
            camRot.x -= 360;
        else if (camRot.x < -90)
            camRot.x += 360;
    }
}
