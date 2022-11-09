using UnityEngine;
using UnityEngine.InputSystem;

public class Placer : MonoBehaviour
{
    [SerializeField] Vector3 position;
    [SerializeField] Transform cam;
    [SerializeField] float range;
    [SerializeField] LayerMask placementMask;

    Item active;
    bool placing;
    int layer;

    int ignoreLayer;

    void Awake()
    {
        ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void SetItem(Item item)
    {
        active = item;

        item.transform.parent = transform;
        item.transform.localPosition = position;

        item.gameObject.SetActive(true);

        layer = item.gameObject.layer;
        item.gameObject.layer = ignoreLayer;
    }

    void Update()
    {
        if (!active) return;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range, placementMask))
        {
            if (Mouse.current.rightButton.isPressed)
            {
                placing = true;
                active.transform.position = hit.point;
            }
            else if (placing)
            {
                placing = false;
                active.Release();
                active.gameObject.layer = layer;
                active.transform.parent = null;
                active = null;
            }
        }
        else
        {
            placing = false;
            active.transform.localPosition = position;
        }
    }
}
