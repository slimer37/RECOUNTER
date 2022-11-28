using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Item : Interactable
{
    [SerializeField] Renderer rend;
    [SerializeField] bool isCylindrical;

    [Header("Optional")]
    [SerializeField] Rigidbody rb;
    [SerializeField] bool isThrowable;
    [SerializeField] Vector3 holdPosShift;
    [SerializeField] bool overridesHoldRot;
    [SerializeField] Vector3 holdRot;

    [Header("Bounds Override")]
    [SerializeField] bool overridesBounds;
    [SerializeField] Vector3 overrideCenter;
    [SerializeField, Min(0)] Vector3 overrideSize;

    Collider[] colliders;
    Hotbar containerHotbar;

    public Vector3 HoldPosShift => holdPosShift;
    public Vector3 OriginShift => GetOriginShift();
    public Quaternion? OverrideHoldRotation => overridesHoldRot ? Quaternion.Euler(holdRot) : null;

    public bool IsHeld => containerHotbar;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(rend.localBounds.center, rend.localBounds.size);

        if (!overridesBounds) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(overrideCenter, overrideSize);
    }

    void Reset()
    {
        TryGetComponent(out rend);
        TryGetComponent(out rb);
    }

    void OnValidate()
    {
        if (!rb && isThrowable)
        {
            Debug.LogWarning($"Cannot set {isThrowable} if no rigidbody is selected.");
            isThrowable = false;
        }
    }

    public void Throw(Vector3 force)
    {
        Release();
        rb.AddForce(force, ForceMode.VelocityChange);
    }

    Vector3 GetScaledExtents() => overridesBounds ?
        overrideSize / 2 : Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

    Vector3 GetOriginShift() => overridesBounds ?
        overrideCenter : rend.localBounds.center;

    public bool WouldIntersectAt(Vector3 position, Quaternion rotation, LayerMask mask)
    {
        var scaledExtents = GetScaledExtents();
        position += rotation * GetOriginShift();

        var intersects = Physics.CheckBox(position, scaledExtents, rotation, mask);

        if (intersects && isCylindrical)
        {
            var radius = Mathf.Max(scaledExtents.x, scaledExtents.z);
            intersects = Physics.CheckCapsule(position - Vector3.up * scaledExtents.y, position + Vector3.up * scaledExtents.y, radius, mask);
        }

        return intersects;
    }

    public float SizeAlong(Vector3 localDirection)
    {
        var scaledExtents = GetScaledExtents();
        var originShift = Vector3.Dot(localDirection, GetOriginShift());

        if (isCylindrical)
        {
            var radialComponent = new Vector2(localDirection.x, localDirection.z).magnitude;
            return originShift + Mathf.Max(scaledExtents.x, scaledExtents.z) * radialComponent + scaledExtents.y * localDirection.y;
        }
        else
        {
            var absDirection = new Vector3(Mathf.Abs(localDirection.x), Mathf.Abs(localDirection.y), Mathf.Abs(localDirection.z));
            return originShift + Vector3.Dot(absDirection, scaledExtents);
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
