using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum Icon { None, Access, Door, Eye, Hand, Invalid, Pickup, Pull, Push }

    public struct HudInfo
    {
        public Icon icon;
        public string text;
    }

    public static readonly HudInfo DefaultInteractHud = new() { icon = Icon.Access, text = "Interact" };
    public static readonly HudInfo BlankHud = new() { icon = Icon.None };

    public virtual HudInfo GetHudInfo(Employee e) => CanInteract(e) ? DefaultInteractHud : BlankHud;

    public virtual bool CanInteract(Employee e) => true;

    public abstract void Interact(Employee e);

    public virtual void InteractEnd() { }

    public virtual void OnHover(bool hover) { }
}
