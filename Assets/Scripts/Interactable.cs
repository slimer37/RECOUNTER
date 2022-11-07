using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected virtual void OnValidate()
    {
        if (LayerMask.LayerToName(gameObject.layer) != "Interactable")
            Debug.LogWarning(name + " is not on the Interactable layer.", this);
    }

    public virtual string GetText() => "Interact";
    public virtual bool CanInteract() => true;

    public abstract void Interact();
    public virtual void OnHover(bool hover) => Debug.Log("hover " + hover);
}
