using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Moving")]
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
    [SerializeField] float jumpForce;
    [SerializeField] float gravity;
    [SerializeField] float lowJumpMultiplier;

    [Header("Looking")]
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
    float time;

    bool isSuspended;

    void Update()
    {
        if (Pause.IsPaused) return;

        // Jumping & Gravity

        if (controller.isGrounded)
        {
            if (playerControls.Jump.WasPressedThisFrame())
                yVelocity = jumpForce;
        }
        else if (yVelocity > 0 && !playerControls.Jump.IsPressed())
        {
            yVelocity -= gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            yVelocity -= gravity * Time.deltaTime;
        }

        // Movement

        var move = playerControls.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        var isSprinting = playerControls.Sprint.IsPressed() && move.y > 0;

        if (move.sqrMagnitude > 0)
        {
            var impulse = isSprinting ? sprintImpulse : walkImpulse;

            if (time == 0)
                impulse.GenerateImpulse();

            time += Time.deltaTime;

            var impulseInterval = impulse.m_ImpulseDefinition.m_ImpulseDuration;
            impulseInterval += isSprinting ? sprintImpulseInterval : walkImpulseInterval;

            if (time > impulseInterval)
            {
                impulse.GenerateImpulse();
                time = 0;
            }
        }
        else
        {
            time = 0;
        }

        var speed = isSprinting ? sprintSpeed : walkSpeed;

        var velocity = body.TransformDirection(move.x, 0, move.y) * speed + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);

        // FOV change

        var targetFov = isSprinting ? sprintFov : walkFov;
        fov = Mathf.Lerp(fov, targetFov, fovChangeSpeed * Time.deltaTime);
        vcam.m_Lens.FieldOfView = fov;

        // Look rotation

        var look = playerControls.Look.ReadValue<Vector2>() * sensitivity;

        camRot.y += look.x;
        camRot.x -= look.y;
        camRot.x = Mathf.Clamp(camRot.x, -clamp, clamp);

        body.eulerAngles = camRot.y * Vector3.up;
        camTarget.localEulerAngles = camRot.x * Vector3.right;
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
        isSuspended = suspend;

        if (suspend)
            OnDisable();
        else
            OnEnable();
    }
}
