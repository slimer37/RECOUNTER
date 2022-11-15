using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Placer : MonoBehaviour
{
    [SerializeField] Vector3 holdPosition;
    [SerializeField] Transform cam;
    [SerializeField] float range;
    [SerializeField] float rotateSpeed;
    [SerializeField] float surfaceSeparation;
    [SerializeField] LayerMask placementMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteraction playerInteraction;
    [SerializeField] Ghost ghost;
    [SerializeField, Layer] int heldItemLayer;

    [Header("UI")]
    [SerializeField] Sprite defaultIcon;
    [SerializeField] Sprite placeIcon;
    [SerializeField] Sprite rotateIcon;
    [SerializeField] Image icon;

    Item active;
    float itemRotation;

    bool placing;
    int originalLayer;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(holdPosition, Vector3.one * 0.4f);
    }

    public void SetItem(Item item)
    {
        active = item;

        item.transform.localPosition = holdPosition;

        item.gameObject.SetActive(true);

        originalLayer = item.gameObject.layer;
        item.gameObject.layer = heldItemLayer;

        ghost.CopyMesh(item);

        MoveActiveToHand();
    }

    void Update()
    {
        if (!active) return;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range, placementMask))
        {
            if (Mouse.current.rightButton.isPressed)
            {
                StartPlace();

                if (Mouse.current.leftButton.isPressed)
                {
                    playerController.enabled = false;
                    icon.sprite = rotateIcon;

                    var mouseX = Mouse.current.delta.ReadValue().x;
                    itemRotation += mouseX * rotateSpeed * Time.deltaTime;
                }
                else
                {
                    playerController.enabled = true;
                    icon.sprite = placeIcon;
                }

                var position = hit.point + hit.normal * (surfaceSeparation + active.SizeAlong(hit.normal));
                var rotation = Quaternion.Euler(Vector3.up * itemRotation);

                if (active.WouldIntersectAt(position, rotation, obstacleMask))
                {
                    ghost.ShowAt(position, rotation);
                    MoveActiveToHand();
                }
                else
                {
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

    void StartPlace()
    {
        if (placing) return;

        playerInteraction.enabled = false;
        placing = true;

        active.transform.parent = null;

        icon.sprite = placeIcon;

        active.gameObject.layer = originalLayer;
    }

    void EndPlace()
    {
        if (!placing) return;

        playerInteraction.enabled = true;
        placing = false;

        active.transform.parent = null;

        icon.sprite = defaultIcon;

        ghost.Hide();

        active.gameObject.layer = heldItemLayer;
    }

    void DropItem()
    {
        if (!placing) return;

        if (!active) throw new InvalidOperationException("No active item to drop.");

        EndPlace();

        active.Release();
        active.gameObject.layer = originalLayer;

        Inventory.Instance.RemoveItem(active);
        active = null;
    }

    void MoveActiveToHand()
    {
        active.transform.localRotation = Quaternion.identity;
        active.transform.parent = transform;
        active.transform.localPosition = holdPosition;
    }
}
