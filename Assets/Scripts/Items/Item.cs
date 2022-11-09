public class Item : Interactable
{
    public bool IsHeld { get; private set; }

    public override string GetText() => IsHeld ? "" : "Pick up";
    public override Icon GetIcon() => IsHeld ? default : Icon.Pickup;

    public override bool CanInteract() => !IsHeld;

    public override void Interact()
    {
        IsHeld = Inventory.Instance.TryAddItem(this);
    }

    public void Release()
    {
        IsHeld = false;
    }
}
