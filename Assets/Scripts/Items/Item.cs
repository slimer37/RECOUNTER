using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Renderer))]
public class Item : Interactable
{
    [SerializeField] Renderer rend;
    [SerializeField] Collider col;
    [SerializeField] bool isCylindrical;

    [Header("Optional")]
    [SerializeField] Rigidbody rb;

    public bool IsHeld { get; private set; }

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
        TryGetComponent(out col);

        if (col is CapsuleCollider or SphereCollider)
            isCylindrical = true;

        TryGetComponent(out rend);
        TryGetComponent(out rb);
    }

    public float SizeAlong(Vector3 direction)
    {
        direction = transform.InverseTransformDirection(direction);
        var scaledExtents = Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

        if (isCylindrical)
        {
            var radialComponent = new Vector2(direction.x, direction.z).magnitude;
            return Mathf.Max(scaledExtents.x, scaledExtents.z) * radialComponent + scaledExtents.y * direction.y;
        }
        else
        {
            var absDirection = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            return Vector3.Dot(absDirection, scaledExtents);
        }
    }

    public override string GetText() => IsHeld ? "" : "Pick up";
    public override Icon GetIcon() => IsHeld ? default : Icon.Pickup;

    public override bool CanInteract() => !IsHeld;

    public override void Interact()
    {
        if (!Inventory.Instance.TryAddItem(this)) return;

        IsHeld = true;
        col.enabled = false;

        if (rb)
            rb.isKinematic = true;
    }

    public void Release()
    {
        IsHeld = false;
        col.enabled = true;

        if (rb)
            rb.isKinematic = false;
    }
}
