using UnityEngine;

public class Item : Interactable
{
    [SerializeField] Collider col;
    [SerializeField] Renderer rend;
    [SerializeField] Rigidbody rb;

    public bool IsHeld { get; private set; }

    public float SizeAlong(Vector3 direction)
    {
        direction = transform.InverseTransformDirection(direction);
        return Vector3.Dot(new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z)), Vector3.Scale(transform.localScale, rend.localBounds.extents));
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
