using UnityEngine;
using UnityEngine.InputSystem;

public class Placer : MonoBehaviour
{
    [SerializeField] Vector3 holdPosition;
    [SerializeField] Transform cam;
    [SerializeField] float range;
    [SerializeField] float rotateSpeed;
    [SerializeField] LayerMask placementMask;
    [SerializeField] PlayerController playerController;

    Item active;
    bool placing;
    int layer;

    int ignoreLayer;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(holdPosition, Vector3.one * 0.4f);
    }

    void Awake()
    {
        ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void SetItem(Item item)
    {
        active = item;

        item.transform.localPosition = holdPosition;

        item.gameObject.SetActive(true);

        layer = item.gameObject.layer;
        item.gameObject.layer = ignoreLayer;

        active.transform.localRotation = Quaternion.identity;
        active.transform.parent = transform;
        active.transform.localPosition = holdPosition;
    }

    void LateUpdate()
    {
        if (!active) return;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range, placementMask))
        {
            if (Mouse.current.rightButton.isPressed)
            {
                playerController.enabled = true;
                placing = true;
                active.transform.parent = null;

                if (Mouse.current.leftButton.isPressed)
                {
                    var x = Mouse.current.delta.ReadValue().x;
                    active.transform.Rotate(Vector3.up, -x * rotateSpeed * Time.deltaTime);
                    playerController.enabled = false;
                }

                active.transform.position = hit.point + hit.normal * active.SizeAlong(hit.normal);
            }
            else if (placing)
            {
                placing = false;
                active.Release();
                active.gameObject.layer = layer;

                Inventory.Instance.RemoveItem(active);
                active = null;
                playerController.enabled = true;
            }
        }
        else
        {
            placing = false;
            active.transform.localRotation = Quaternion.identity;
            active.transform.parent = transform;
            active.transform.localPosition = holdPosition;
        }
    }
}
