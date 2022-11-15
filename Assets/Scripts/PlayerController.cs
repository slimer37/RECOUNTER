using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

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

    CharacterController controller;
    Controls.PlayerActions playerControls;

    Vector2 camRot;
    float fov;
    float yVelocity;

    void Update()
    {
        if (Pause.IsPaused) return;

        // Jumping & Gravity

        yVelocity -= gravity * Time.deltaTime;

        if (yVelocity > 0 && !playerControls.Jump.IsPressed())
            yVelocity -= gravity * lowJumpMultiplier * Time.deltaTime;

        var jump = controller.isGrounded && playerControls.Jump.WasPressedThisFrame();

        if (jump)
            yVelocity = jumpForce;

        // Movement

        var move = playerControls.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        var isSprinting = playerControls.Sprint.IsPressed() && move.y > 0;

        var speed = isSprinting ? sprintSpeed : walkSpeed;

        var velocity = transform.TransformDirection(move.x, 0, move.y) * speed + Vector3.up * yVelocity;

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

        transform.eulerAngles = camRot.y * Vector3.up;
        camTarget.localEulerAngles = camRot.x * Vector3.right;
    }

    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        fov = walkFov;

        camRot.x = transform.localEulerAngles.y;
        camRot.y = camTarget.localEulerAngles.x;

        controller = GetComponent<CharacterController>();
        playerControls = new Controls().Player;
    }

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();
}
