using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Placer : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] float sensitivity;
    [SerializeField] float range;
    [SerializeField] float rotateSpeed;
    [SerializeField] float surfaceSeparation;
    [SerializeField] float tiltEffect;
    [SerializeField] float clampTiltX;
    [SerializeField] LayerMask placementMask;
    [SerializeField] LayerMask obstacleMask;

    [Header("Viewmodel Settings")]
    [SerializeField, Layer] int heldItemLayer;
    [SerializeField, Layer] int defaultLayer;
    [SerializeField] Vector3 holdPosition;

    [Header("Components")]
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteraction playerInteraction;
    [SerializeField] Camera cam;
    [SerializeField] Transform camTarget;
    [SerializeField] Transform body;
    [SerializeField] Ghost ghost;

    [Header("UI")]
    [SerializeField] Sprite defaultIcon;
    [SerializeField] Sprite placeIcon;
    [SerializeField] Sprite rotateIcon;
    [SerializeField] Image icon;

    Item active;
    float itemRotation;
    Vector2 mousePosition;

    Vector3 playerRotation;

    bool placing;
    bool rotating;

    Controls.PlayerActions playerControls;

    public Item Active => active;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cam.transform.position, range);
    }

    void Awake() => playerControls = new Controls().Player;

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();

    public void SetItem(Item item)
    {
        active = item;

        active.gameObject.SetActive(true);

        ghost.CopyMesh(item);

        active.transform.parent = transform;

        MoveActiveToHand();
    }

    public void StopHoldingItem()
    {
        if (!active) return;

        EndPlace();

        SetLayer(false);
        active.gameObject.SetActive(false);

        active = null;
    }

    void Update()
    {
        if (Pause.IsPaused) return;

        icon.sprite = placing switch
        {
            true when rotating => rotateIcon,
            true => placeIcon,
            _ => defaultIcon
        };

        if (!active) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            mousePosition = new Vector2(Screen.width, Screen.height) / 2;
            playerRotation = new Vector3(camTarget.localEulerAngles.x, body.localEulerAngles.y, 0);
        }

        if (Mouse.current.rightButton.isPressed)
        {
            var ray = cam.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out var hit, range, placementMask))
            {
                hit.point = ray.GetPoint(range);
            }

            HandleRotation(out var rotation);

            if (!rotating)
            {
                mousePosition += playerControls.Look.ReadValue<Vector2>() * sensitivity;
                mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
                mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);
            }

            StartPlace();

            TiltCamera();

            var position = hit.point + hit.normal * (surfaceSeparation + active.SizeAlong(rotation * hit.normal));

            if (active.WouldIntersectAt(position, rotation, obstacleMask))
            {
                ghost.ShowAt(position, rotation);

                MoveActiveToHand();
            }
            else
            {
                SetLayer(false);
                active.transform.SetPositionAndRotation(position, rotation);

                ghost.Hide();
            }
        }
        else if (placing)
        {
            DropItem();
        }
        else
        {
            EndPlace();
            MoveActiveToHand();
        }
    }

    void TiltCamera()
    {
        var normalizedMouse = 2 * mousePosition / new Vector2(Screen.width, Screen.height) - Vector2.one;
        var tilt = new Vector3(-normalizedMouse.y, normalizedMouse.x, 0) * tiltEffect;
        var angles = playerRotation + tilt;
        angles.x = Mathf.Clamp(angles.x, -clampTiltX, clampTiltX);

        body.eulerAngles = angles.y * Vector3.up;
        camTarget.localEulerAngles = angles.x * Vector3.right;
    }

    void HandleRotation(out Quaternion rotation)
    {
        rotating = Mouse.current.leftButton.isPressed;

        if (rotating)
        {
            var mouseX = Mouse.current.delta.ReadValue().x;
            itemRotation += mouseX * rotateSpeed * Time.deltaTime;
        }

        rotation = Quaternion.Euler(Vector3.up * itemRotation);
    }

    void StartPlace()
    {
        playerInteraction.enabled = false;
        playerController.enabled = false;
        placing = true;

        icon.transform.position = mousePosition;
    }

    void EndPlace()
    {
        playerInteraction.enabled = true;
        playerController.enabled = true;
        placing = false;

        icon.transform.position = new Vector2(Screen.width, Screen.height) / 2;

        ghost.Hide();
    }

    void DropItem()
    {
        if (!active) throw new InvalidOperationException("No active item to drop.");

        EndPlace();

        SetLayer(false);
        active.transform.parent = null;

        var temp = active;

        active = null;

        temp.Release();
    }

    void MoveActiveToHand()
    {
        SetLayer(true);
        active.transform.localRotation = Quaternion.identity;
        active.transform.localPosition = holdPosition;
    }

    void SetLayer(bool heldLayer)
    {
        var layer = heldLayer ? heldItemLayer : defaultLayer;
        active.gameObject.layer = layer;

        foreach (Transform child in active.transform)
        {
            child.gameObject.layer = layer;
        }
    }
}
