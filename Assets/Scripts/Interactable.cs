using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public bool IsInteractionInProgress { get; private set; }
    protected Employee Interactor { get; private set; }

    public enum Icon { None, Access, Door, Eye, Hand, Invalid, Pickup, Pull, Push }

    public struct HudInfo
    {
        public Icon icon;
        public string text;
    }

    public static readonly HudInfo DefaultInteractHud = new() { icon = Icon.Access, text = "Interact" };
    public static readonly HudInfo BlankHud = new() { icon = Icon.None };

    /// <summary>
    /// Forms the HUD elements that should be shown while this object is hovered.
    /// </summary>
    public virtual HudInfo GetHudInfo(Employee e) => CanInteract(e) ? DefaultInteractHud : BlankHud;
    
    /// <summary>
    /// Checks if an object can be interacted with.
	/// </summary>
	/// <param name="e">
    /// The player that would initiate the interaction.
    /// </param>
    public virtual bool CanInteract(Employee e) => true;

    public void Interact(Employee e)
    {
        IsInteractionInProgress = true;
        Interactor = e;
        OnInteract(e);
    }

    public void EndInteract()
    {
        if (!IsInteractionInProgress) return;

        IsInteractionInProgress = false;
        Interactor = null;
        OnEndInteraction();
    }

    /// <summary>
    /// The main interaction method of an object. Called once on interaction.
	/// </summary>
	/// <remarks>
	/// Will only be called if <c>CanInteract</c> returns true.
	/// </remarks>
	/// <param name="e">
    /// The player initiating the interaction.
    /// </param>
    protected virtual void OnInteract(Employee e) { }

    /// <summary>
    /// Called when the interact key is released or the object loses hover while the key is held.
    /// </summary>
    /// <remarks>
    /// Guaranteed to follow one call to <c>Interact</c>.
    /// </remarks>
    protected virtual void OnEndInteraction() { }

    public virtual void OnHover(bool hover) { }
}
