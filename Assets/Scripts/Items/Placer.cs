using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Placer : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] float range;
    [SerializeField] float rotateSpeed;
    [SerializeField] float surfaceSeparation;
    [SerializeField] LayerMask placementMask;
    [SerializeField] LayerMask obstacleMask;

    [Header("Viewmodel Settings")]
    [SerializeField, Layer] int heldItemLayer;
    [SerializeField] Vector3 holdPosition;

    [Header("Components")]
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteraction playerInteraction;
    [SerializeField] Transform cam;
    [SerializeField] Ghost ghost;

    [Header("UI")]
    [SerializeField] Sprite defaultIcon;
    [SerializeField] Sprite placeIcon;
    [SerializeField] Sprite rotateIcon;
    [SerializeField] Image icon;

    Item active;
    float itemRotation;

    bool placing;
    int originalLayer;

    public Item Active => active;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(holdPosition, Vector3.one * 0.4f);
    }

    public void SetItem(Item item)
    {
        active = item;

        active.gameObject.SetActive(true);

        originalLayer = item.gameObject.layer;

        ghost.CopyMesh(item);

        active.transform.parent = transform;

        MoveActiveToHand();
    }

    public void StopHoldingItem()
    {
        if (!active) return;

        EndPlace();

        active.gameObject.SetActive(false);

        active = null;
    }

    void Update()
    {
        if (Pause.IsPaused) return;

        if (!active) return;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range, placementMask))
        {
            if (Mouse.current.rightButton.isPressed)
            {
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
                    active.gameObject.layer = originalLayer;
                    active.transform.SetPositionAndRotation(position, rotation);

                    ghost.Hide();
                }
            }
            else if (placing)
            {
                DropItem();
            }
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

        playerController.enabled = !rotating;

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
        placing = true;

        icon.sprite = placeIcon;
    }

    void EndPlace()
    {
        if (!placing) return;

        playerInteraction.enabled = true;
        playerController.enabled = true;
        placing = false;

        icon.sprite = defaultIcon;

        ghost.Hide();
    }

    void DropItem()
    {
        if (!placing) return;

        if (!active) throw new InvalidOperationException("No active item to drop.");

        EndPlace();

        active.gameObject.layer = originalLayer;
        active.transform.parent = null;

        var temp = active;

        active = null;

        temp.Release();
    }

    void MoveActiveToHand()
    {
        active.gameObject.layer = heldItemLayer;
        active.transform.localRotation = Quaternion.identity;
        active.transform.localPosition = holdPosition;
    }
}
