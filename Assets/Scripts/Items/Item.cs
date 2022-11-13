using UnityEngine;

public class Item : Interactable
{
    [SerializeField] Renderer rend;
    [SerializeField] Collider col;
    [SerializeField] bool isCylindrical;

    [Header("Optional")]
    [SerializeField] Rigidbody rb;

    public bool IsHeld { get; private set; }

    public float SizeAlong(Vector3 direction)
    {
        direction = transform.InverseTransformDirection(direction);
        var scaledExtents = Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

        if (isCylindrical)
        {
            var radialComponent = new Vector2(direction.x, direction.z).magnitude;
            return scaledExtents.x * radialComponent + scaledExtents.y * direction.y;
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
        if (rb)
            rb.isKinematic = true;
    }

    public void Release()
    {
        IsHeld = false;
        if (rb)
            rb.isKinematic = false;
    }
}
