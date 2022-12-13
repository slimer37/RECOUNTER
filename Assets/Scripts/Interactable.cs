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
	/// <param name="e">The player that would interact.</param>
    public virtual bool CanInteract(Employee e) => true;

    /// <summary>
    /// Interacts with this object and records the interactor.
    /// </summary>
    /// <param name="e">The interacting player.</param>
    public void Interact(Employee e)
    {
        IsInteractionInProgress = true;
        Interactor = e;
        OnInteract(e);
    }

    /// <summary>
    /// Ends the interaction in progress.
    /// </summary>
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
	/// Will only be called if <see cref="CanInteract(Employee)"/> returns true.
	/// </remarks>
	/// <param name="e">The interacting player.</param>
    protected virtual void OnInteract(Employee e) { }

    /// <summary>
    /// Called when the interact key is released or the object loses hover while the key is held.
    /// </summary>
    /// <remarks>
    /// Guaranteed to follow one call to <see cref="Interact(Employee)"/>.
    /// </remarks>
    protected virtual void OnEndInteraction() { }

    /// <summary>
    /// Directly executes hover events.
    /// </summary>
    /// <param name="hover">True when entering hover, false when exiting.</param>
    public virtual void OnHover(bool hover) { }
}
