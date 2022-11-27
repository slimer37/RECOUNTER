using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Item : Interactable
{
    [SerializeField] Renderer rend;
    [SerializeField] bool isCylindrical;

    [Header("Optional")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 holdPosShift;
    [SerializeField] bool overridesHoldRot;
    [SerializeField] Vector3 holdRot;

    Collider[] colliders;
    Hotbar containerHotbar;

    public Vector3 HoldPosShift => holdPosShift;
    public Quaternion? OverrideHoldRotation => overridesHoldRot ? Quaternion.Euler(holdRot) : null;

    public bool IsHeld => containerHotbar;

    public bool WouldIntersectAt(Vector3 position, Quaternion rotation, LayerMask mask)
    {
        var scaledExtents = Vector3.Scale(transform.lossyScale, rend.localBounds.extents);
        if (isCylindrical)
        {
            var radius = Mathf.Max(scaledExtents.x, scaledExtents.z);
            scaledExtents.y -= radius;
            return Physics.CheckCapsule(position - Vector3.up * scaledExtents.y, position + Vector3.up * scaledExtents.y, radius, mask);
        }
        else
        {
            return Physics.CheckBox(position, scaledExtents, rotation, mask);
        }
    }

    void Reset()
    {
        TryGetComponent(out rend);
        TryGetComponent(out rb);
    }

    public float SizeAlong(Vector3 localDirection)
    {
        var scaledExtents = Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

        if (isCylindrical)
        {
            var radialComponent = new Vector2(localDirection.x, localDirection.z).magnitude;
            return Mathf.Max(scaledExtents.x, scaledExtents.z) * radialComponent + scaledExtents.y * localDirection.y;
        }
        else
        {
            var absDirection = new Vector3(Mathf.Abs(localDirection.x), Mathf.Abs(localDirection.y), Mathf.Abs(localDirection.z));
            return Vector3.Dot(absDirection, scaledExtents);
        }
    }

    public override HudInfo GetHudInfo(Employee e) => CanInteract(e) ? new HudInfo { icon = Icon.Pickup, text = "Pick up" } : BlankHud;

    public override bool CanInteract(Employee e) => !IsHeld && !e.ItemHotbar.IsActiveSlotFull;

    public override void Interact(Employee e)
    {
        if (!e.ItemHotbar.TryAddItem(this)) return;

        containerHotbar = e.ItemHotbar;
        EnableColliders(false);

        if (rb)
            rb.isKinematic = true;
    }

    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Release()
    {
        containerHotbar.RemoveItem(this);

        containerHotbar = null;
        EnableColliders(true);

        if (rb)
            rb.isKinematic = false;
    }

    void EnableColliders(bool enable)
    {
        foreach (var col in colliders)
        {
            col.enabled = enable;
        }
    }
}
