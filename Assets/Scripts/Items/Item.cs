using UnityEngine;
using NaughtyAttributes;
using Recounter.Thumbnails;

public class Item : Interactable
{
    [SerializeField] Renderer rend;
    [SerializeField] bool isCylindrical;

    [Header("Optional")]
    [SerializeField] Rigidbody rb;
    [SerializeField, ShowIf(nameof(HasRigidbody))] bool isThrowable = true;
    [SerializeField] Vector3 holdPosShift;
    [SerializeField] bool overridesHoldRot;
    [SerializeField, ShowIf(nameof(overridesHoldRot))] Vector3 holdRot;

    [field: Header("Viewmodel")]
    [field: SerializeField] public ViewmodelPose ViewmodelPose { get; private set; }

    [Header("Bounds Override")]
    [SerializeField] bool overridesBounds;
    [SerializeField, ShowIf(nameof(overridesBounds))] Vector3 overrideCenter;
    [SerializeField, Min(0), ShowIf(nameof(overridesBounds))] Vector3 overrideSize;

    Collider[] colliders;
    Hotbar containerHotbar;

    public Vector3 HoldPosShift => holdPosShift;
    public Vector3 OriginShift => GetOriginShift();
    public Quaternion? OverrideHoldRotation => overridesHoldRot ? Quaternion.Euler(holdRot) : null;

    public bool IsHeld => containerHotbar;
    public bool IsThrowable => rb && isThrowable;
    public bool HasRigidbody => rb;

    public Texture2D Thumbnail =>
            _thumbnail ? _thumbnail : _thumbnail = ThumbnailCreator.CreateThumbnail(transform);

    Texture2D _thumbnail;

    bool justReleased;

    void OnDrawGizmosSelected()
    {
        if (!rend) return;

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
            Debug.LogWarning($"Cannot set {nameof(isThrowable)} if no rigidbody is selected.");
            isThrowable = false;
        }
    }

    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Throw(Vector3 force)
    {
        Release();
        rb.AddForce(force, ForceMode.Impulse);
    }

    Vector3 GetScaledExtents() => overridesBounds ?
        overrideSize / 2 : Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

    Vector3 GetOriginShift() => overridesBounds ?
        overrideCenter : rend.localBounds.center;

    public bool IsIntersecting(LayerMask mask) =>
        WouldIntersectAt(transform.position, transform.rotation, mask);

    public bool WouldIntersectAt(Vector3 position, Quaternion rotation, LayerMask mask)
    {
        var scaledExtents = GetScaledExtents();
        position += Quaternion.Inverse(rotation) * GetOriginShift();

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
            return originShift + Mathf.Max(scaledExtents.x, scaledExtents.z) * radialComponent + scaledExtents.y * Mathf.Abs(localDirection.y);
        }
        else
        {
            var absDirection = new Vector3(Mathf.Abs(localDirection.x), Mathf.Abs(localDirection.y), Mathf.Abs(localDirection.z));
            return originShift + Vector3.Dot(absDirection, scaledExtents);
        }
    }

    protected override HudInfo FormHud(Employee e) => new() { icon = Icon.Pickup, text = "Pick up" };

    protected override bool CanInteract(Employee e) => !IsHeld && !e.ItemHotbar.IsActiveSlotFull;

    protected override void OnInteract(Employee e)
    {
        e.ItemHotbar.TryAddItem(this);
    }
    
    public void PickUp(Hotbar hotbar)
    {
        containerHotbar = hotbar;
        EnableColliders(false);

        if (rb)
            rb.isKinematic = true;
    }

    public void Release()
    {
        justReleased = true;

        containerHotbar.RemoveItem(this);

        containerHotbar = null;
        EnableColliders(true);

        if (rb)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (justReleased)
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    void EnableColliders(bool enable)
    {
        foreach (var col in colliders)
        {
            col.enabled = enable;
        }
    }
}
