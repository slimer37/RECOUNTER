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
    [SerializeField] Ghost ghost;

    [Header("UI")]
    [SerializeField] Sprite defaultIcon;
    [SerializeField] Sprite placeIcon;
    [SerializeField] Sprite rotateIcon;
    [SerializeField] Image icon;

    Item active;
    float itemRotation;
    Vector2 mousePosition;

    bool placing;

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

        if (!active) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
            mousePosition = new Vector2(Screen.width, Screen.height) / 2;

        if (Mouse.current.rightButton.isPressed)
        {
            var ray = cam.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out var hit, range, placementMask))
            {
                hit.point = ray.GetPoint(range);
            }

            mousePosition += playerControls.Look.ReadValue<Vector2>() * sensitivity;
            mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
            mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);

            StartPlace();

            HandleRotation(out var rotation);

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

    void HandleRotation(out Quaternion rotation)
    {
        var rotating = Mouse.current.leftButton.isPressed;

        if (rotating)
        {
            var mouseX = Mouse.current.delta.ReadValue().x;
            itemRotation += mouseX * rotateSpeed * Time.deltaTime;
            icon.sprite = rotateIcon;
        }

        rotation = Quaternion.Euler(Vector3.up * itemRotation);
    }

    void StartPlace()
    {
        playerInteraction.enabled = false;
        playerController.enabled = false;
        placing = true;

        icon.sprite = placeIcon;
    }

    void EndPlace()
    {
        playerInteraction.enabled = true;
        playerController.enabled = true;
        placing = false;

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
