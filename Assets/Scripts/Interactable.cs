using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual string GetText() => "Interact";
    public virtual bool CanInteract() => true;

    public abstract void Interact();
    public virtual void OnHover(bool hover) => Debug.Log("hover " + hover);
}
