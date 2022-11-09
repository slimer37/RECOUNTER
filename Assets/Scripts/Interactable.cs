using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum Icon
    {
        None,
        Access,
        Door,
        Eye,
        Hand,
        Invalid,
        Pickup,
        Pull,
        Push
    }

    public virtual Icon GetIcon() => Icon.Access;
    public virtual string GetText() => "Interact";
    public virtual bool CanInteract() => true;

    public abstract void Interact();
    public virtual void OnHover(bool hover) { }
}
