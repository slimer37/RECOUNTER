using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform camTarget;
    [SerializeField] float speed;
    [SerializeField] float sensitivity;
    [SerializeField] float clamp;

    CharacterController controller;
    Controls.PlayerActions playerControls;

    Vector2 camRot;

    void Update()
    {
        var move = playerControls.Move.ReadValue<Vector2>();
        var look = playerControls.Look.ReadValue<Vector2>() * sensitivity;

        controller.Move(transform.TransformDirection(move.x, 0, move.y) * speed * Time.deltaTime);

        camRot.y += look.x;
        camRot.x -= look.y;
        camRot.x = Mathf.Clamp(camRot.x, -clamp, clamp);

        transform.eulerAngles = camRot.y * Vector3.up;
        camTarget.localEulerAngles = camRot.x * Vector3.right;
    }

    void Awake()
    {
        camRot.x = transform.localEulerAngles.y;
        camRot.y = camTarget.localEulerAngles.x;

        controller = GetComponent<CharacterController>();
        playerControls = new Controls().Player;
    }

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();
}
