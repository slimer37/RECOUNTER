using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

    [Header("Looking")]
    [SerializeField] Transform camTarget;
    [SerializeField] float sensitivity;
    [SerializeField] float clamp;
    [SerializeField] float walkFov;
    [SerializeField] float sprintFov;
    [SerializeField] float fovChangeSpeed;
    [SerializeField] CinemachineVirtualCamera vcam;

    CharacterController controller;
    Controls.PlayerActions playerControls;

    Vector2 camRot;
    float fov;

    void Update()
    {
        // Movement

        var move = playerControls.Move.ReadValue<Vector2>();

        // Sprinting only allowed when moving forward
        var isSprinting = playerControls.Sprint.IsPressed() && move.y > 0;

        var speed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(transform.TransformDirection(move.x, 0, move.y) * speed * Time.deltaTime);

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
