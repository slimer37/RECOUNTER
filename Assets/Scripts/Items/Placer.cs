using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Placer : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClipGroup holdSounds;
    [SerializeField] AudioSource source;

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
    [SerializeField] float smoothing;

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
    Vector3 itemVelocity;
    float itemRotationVelocity;

    Vector2 mousePosition;

    Vector3 playerRotation;

    bool placing;
    bool rotating;
    bool itemIntersects;

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

    public void SetItem(Item item, bool resetPosition)
    {
        source.PlayOneShot(holdSounds.NextClip());

        active = item;

        active.gameObject.SetActive(true);

        ghost.CopyMesh(item);

        if (!resetPosition) return;

        active.transform.position = body.position;
        active.transform.rotation = body.rotation;
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

        if (!active) return;

        if (Mouse.current.rightButton.isPressed)
        {
            if (!placing)
            {
                InitializePlace();
            }

            var ray = cam.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out var hit, range, placementMask))
            {
                hit.point = ray.GetPoint(range);
            }

            HandleRotation(out var rotation);

            if (!rotating)
            {
                TrackMouseLook();
                TiltCamera();
            }

            StartPlace();

            icon.sprite = rotating ? rotateIcon : placeIcon;

            var position = hit.point + hit.normal * (surfaceSeparation + active.SizeAlong(rotation * hit.normal));
            itemIntersects = active.WouldIntersectAt(position, rotation, obstacleMask);

            if (itemIntersects)
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
        else if (placing && !itemIntersects)
        {
            DropItem();
        }
        else
        {
            EndPlace();
            MoveActiveToHand();
        }
    }

    void TrackMouseLook()
    {
        mousePosition += playerControls.Look.ReadValue<Vector2>() * sensitivity;
        mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
        mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);
    }

    void TiltCamera()
    {
        var normalizedMouse = 2 * mousePosition / new Vector2(Screen.width, Screen.height) - Vector2.one;
        var tilt = new Vector3(-normalizedMouse.y, normalizedMouse.x, 0) * tiltEffect;
        var angles = playerRotation + tilt;

        if (angles.x > 90)
            angles.x -= 360;
        else if (angles.x < -90)
            angles.x += 360;

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

            if (mouseX != 0)
            {
                itemRotation += mouseX * rotateSpeed * Time.deltaTime;
                icon.transform.localScale = new Vector3(mouseX > 0 ? 1 : -1, 1, 1);
            }

            icon.transform.localEulerAngles = Vector3.forward * -itemRotation;
        }
        else
        {
            icon.transform.localScale = Vector3.one;
            icon.transform.localEulerAngles = Vector3.zero;
        }

        rotation = Quaternion.Euler(Vector3.up * itemRotation);
    }

    void InitializePlace()
    {
        mousePosition = new Vector2(Screen.width, Screen.height) / 2;
        playerRotation = new Vector3(camTarget.localEulerAngles.x, body.localEulerAngles.y, 0);
    }

    void StartPlace()
    {
        playerInteraction.enabled = false;
        playerController.Suspend(true);
        placing = true;

        icon.transform.position = mousePosition;
    }

    void EndPlace()
    {
        playerInteraction.enabled = true;
        playerController.Suspend(false);
        placing = false;

        icon.transform.position = new Vector2(Screen.width, Screen.height) / 2;
        icon.transform.rotation = Quaternion.identity;
        icon.transform.localScale = Vector3.one;

        icon.sprite = defaultIcon;

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

        var currRot = active.transform.rotation;
        var targetRot = cam.transform.rotation;
        var delta = Quaternion.Angle(currRot, targetRot);
        if (delta > 0f)
        {
            var t = Mathf.SmoothDampAngle(delta, 0, ref itemRotationVelocity, smoothing);
            t = 1f - (t / delta);
            active.transform.rotation = Quaternion.Slerp(currRot, targetRot, t);
        }

        active.transform.position = Vector3.SmoothDamp(
            active.transform.position,
            cam.transform.TransformPoint(holdPosition),
            ref itemVelocity,
            smoothing);
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
