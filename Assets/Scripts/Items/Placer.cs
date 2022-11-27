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
    [SerializeField] float rotateSpeed;
    [SerializeField] float surfaceSeparation;
    [SerializeField] float tiltEffect;
    [SerializeField] float clampTiltX;
    [SerializeField] LayerMask placementMask;
    [SerializeField] LayerMask obstacleMask;

    [Header("Deintersection")]
    [SerializeField] float attemptDistance;
    [SerializeField] int maxAttemptsPerDirection;

    [Header("Range/Scrolling")]
    [SerializeField] float rangeMin;
    [SerializeField] float range;
    [SerializeField] float scrollSpeed;
    [SerializeField] float slowScrollSpeed;
    [SerializeField] float scrollSnappiness;

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

    Vector3 adjustedHoldPos;
    Quaternion adjustedHoldRot;

    Vector2 mousePosition;

    Vector3 playerRotation;

    bool placing;
    bool rotating;
    bool itemIntersects;

    float rawRange;
    float currentRange;

    Controls.PlayerActions playerControls;

    public bool IsPlacing => placing;
    public Item Active => active;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cam.transform.position, range);
    }

    void Awake()
    {
        playerControls = new Controls().Player;
        rawRange = range;
    }

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();

    public void SetItem(Item item, bool resetPosition)
    {
        holdSounds.PlayOneShot(source);

        active = item;

        active.gameObject.SetActive(true);

        ghost.CopyMesh(item);

        adjustedHoldPos = holdPosition + item.HoldPosShift;
        adjustedHoldRot = item.OverrideHoldRotation ?? Quaternion.identity;

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

            if (!Physics.Raycast(ray, out var hit, currentRange, placementMask))
            {
                hit.point = ray.GetPoint(currentRange);
            }

            HandleRotation(out var rotation);

            if (!rotating)
            {
                HandleScroll();
                TrackMouseLook();
                TiltCamera();
            }

            StartPlace();

            icon.sprite = rotating ? rotateIcon : placeIcon;

            var position = hit.point + hit.normal * (surfaceSeparation + active.SizeAlong(rotation * hit.normal));
            itemIntersects = !TryCorrectPosition(position, rotation, hit.normal, out var correctedPos);

            if (itemIntersects)
            {
                ghost.ShowAt(position, rotation);

                MoveActiveToHand();
            }
            else
            {
                SetLayer(false);
                active.transform.SetPositionAndRotation(correctedPos, rotation);

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

    bool TryCorrectPosition(Vector3 pos, Quaternion rot, Vector3 surfaceNormal, out Vector3 corrected)
    {
        corrected = pos;

        // Stop if ghost is already at that position, i.e., we've failed there before.
        if (ghost.transform.position == pos) return false;

        var noNormal = surfaceNormal == Vector3.zero;

        if (!active.WouldIntersectAt(pos, rot, obstacleMask)) return true;

        var tan = Vector3.Cross(noNormal ? Vector3.up : surfaceNormal, body.forward);

        return TryCorrect(pos, rot, noNormal ? Vector3.up : surfaceNormal, out corrected)
            || TryCorrect(pos, rot, tan, out corrected)
            || TryCorrect(pos, rot, -tan, out corrected)
            || TryCorrect(pos, rot, -body.forward, out corrected);
    }

    bool TryCorrect(Vector3 position, Quaternion rotation, Vector3 direction, out Vector3 corrected)
    {
        for (var i = 0; i < maxAttemptsPerDirection; i++)
        {
            var help = (i + 1) * attemptDistance * direction;
            if (!active.WouldIntersectAt(position + help, rotation, obstacleMask))
            {
                corrected = position + help;
                return true;
            }
        }

        corrected = position;
        return false;
    }

    void HandleScroll()
    {
        var scroll = Mouse.current.scroll.ReadValue().y;
        var slow = Keyboard.current.leftShiftKey.IsPressed();

        if (scroll != 0)
        {
            var delta = scroll > 0 ? 1f : -1f;
            delta *= slow ? slowScrollSpeed : scrollSpeed;
            rawRange = Mathf.Clamp(currentRange + delta, rangeMin + active.SizeAlong(Vector3.forward), range);
        }

        currentRange = Mathf.Lerp(currentRange, rawRange, scrollSnappiness);
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
                itemRotation += mouseX * rotateSpeed;
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
        itemRotation = body.eulerAngles.y + 180;
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
        var targetRot = cam.transform.rotation * adjustedHoldRot;
        var delta = Quaternion.Angle(currRot, targetRot);
        if (delta > 0f)
        {
            var t = Mathf.SmoothDampAngle(delta, 0, ref itemRotationVelocity, smoothing);
            t = 1f - (t / delta);
            active.transform.rotation = Quaternion.Slerp(currRot, targetRot, t);
        }

        active.transform.position = Vector3.SmoothDamp(
            active.transform.position,
            cam.transform.TransformPoint(adjustedHoldPos),
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
