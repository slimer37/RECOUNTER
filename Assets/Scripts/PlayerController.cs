using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] bool canMove = true;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] Transform body;
    [SerializeField] CharacterController controller;

    [Header("Footsteps/Bobbing")]
    [SerializeField] CinemachineImpulseSource walkImpulse;
    [SerializeField] float walkImpulseInterval;
    [SerializeField] CinemachineImpulseSource sprintImpulse;
    [SerializeField] float sprintImpulseInterval;

    [Header("Jumping")]
    [SerializeField] bool canJump = true;
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] float lowJumpMultiplier;

    [Header("Looking")]
    [SerializeField] bool canLookAround = true;
    [SerializeField] Transform camTarget;
    [SerializeField] float sensitivity;
    [SerializeField] float clamp;

    [Header("FOV")]
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] float walkFov;
    [SerializeField] float sprintFov;
    [SerializeField] float fovChangeSpeed;

    Controls.PlayerActions playerControls;

    Vector2 camRot;
    float fov;
    float yVelocity;
    float bobTime;

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
        if (!canLookAround) return;

        var look = playerControls.Look.ReadValue<Vector2>() * sensitivity;

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
        if (!canJump || !controller.isGrounded) return;

        if (playerControls.Jump.WasPressedThisFrame())
            yVelocity = jumpForce;
    }

    void ApplyGravity()
    {
        if (controller.isGrounded) return;

        var holdingJump = canJump && playerControls.Jump.IsPressed();

        var velocityChange = gravity * Time.deltaTime;

        if (yVelocity > 0 && !holdingJump)
            velocityChange *= lowJumpMultiplier;

        yVelocity -= velocityChange;
    }

    void HandleMovement()
    {
        var inputMovement = playerControls.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        var isSprinting = playerControls.Sprint.IsPressed() && inputMovement.y > 0;

        HandleBobbing(inputMovement, isSprinting);
        AnimateFov(isSprinting);

        if (!canMove) return;

        var speed = isSprinting ? sprintSpeed : walkSpeed;

        var velocity = body.TransformDirection(inputMovement.x, 0, inputMovement.y) * speed + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleBobbing(Vector2 movement, bool isSprinting)
    {
        if (!controller.isGrounded) return;

        if (movement.sqrMagnitude > 0)
        {
            var impulse = isSprinting ? sprintImpulse : walkImpulse;

            if (bobTime == 0)
                impulse.GenerateImpulse();

            bobTime += Time.deltaTime;

            var impulseInterval = impulse.m_ImpulseDefinition.m_ImpulseDuration;
            impulseInterval += isSprinting ? sprintImpulseInterval : walkImpulseInterval;

            if (bobTime > impulseInterval)
            {
                impulse.GenerateImpulse();
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
    }

    void OnEnable()
    {
        playerControls.Enable();

        camRot.y = body.eulerAngles.y;
        camRot.x = camTarget.localEulerAngles.x;

        if (camRot.x > 90)
            camRot.x -= 360;
        else if (camRot.x < -90)
            camRot.x += 360;
    }

    void OnDisable() => playerControls.Disable();

    public void Suspend(bool suspend)
    {
        if (suspend)
            OnDisable();
        else
            OnEnable();
    }
}
