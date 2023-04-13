using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum Icon
    {
        None, Access, Door, Eye, Hand, Invalid, Pickup, Pull, Push,
        Outlet, Plug, StashPlug, Unplug, Insert, Extract
    }

    [Serializable]
    public struct HudInfo
    {
        public Icon icon;
        public string text;
        public float? fill;
    }

    public bool IsInteractionInProgress { get; private set; }
    protected Employee Interactor { get; private set; }
    protected Employee LastInteractor { get; private set; }

    public static readonly HudInfo DefaultInteractHud = new() { icon = Icon.Access, text = "Interact" };
    public static readonly HudInfo BlankHud = new() { icon = Icon.None };

#if UNITY_EDITOR
    [UnityEditor.InitializeOnEnterPlayMode]
    static void CheckLayers()
    {
        var layer = LayerMask.NameToLayer("Interactable");
        foreach (var i in FindObjectsOfType<Interactable>())
        {
            if (i.gameObject.layer != layer)
            {
                Debug.LogError($"{i.name} has the wrong layer.", i);
            }
        }
    }
#endif

    /// <summary>
    /// Directly executes hover events.
    /// </summary>
    /// <param name="hover">True when entering hover, false when exiting.</param>
    public virtual void OnHover(bool hover) { }

    /// <summary>
    /// Forms the HUD elements that should be shown while this object is hovered.
    /// </summary>
    public HudInfo GetHud(Employee e) => CanInteract(e)
        ? FormHud(e)
        : FormNonInteractHud(e);

    /// <summary>
    /// The HUD to show when CanInteract is true.
    /// Always shown if CanInteract is not overriden.
    /// </summary>
    protected virtual HudInfo FormHud(Employee e) => DefaultInteractHud;

    /// <summary>
    /// The HUD to show when CanInteract is false.
    /// Never shown if CanInteract is not overriden.
    /// </summary>
    protected virtual HudInfo FormNonInteractHud(Employee e) => BlankHud;

    /// <summary>
    /// Checks if an object can be interacted with.
    /// </summary>
    /// <param name="e">The player that would interact.</param>
    protected virtual bool CanInteract(Employee e) => true;

    /// <summary>
    /// Interacts with this object and records the interactor.
    /// </summary>
    /// <param name="e">The interacting player.</param>
    public void Interact(Employee e)
    {
        if (!CanInteract(e)) return;

        IsInteractionInProgress = true;
        LastInteractor = Interactor = e;
        OnInteract(e);
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
    /// Ends the interaction in progress.
    /// </summary>
    public void EndInteract()
    {
        if (!IsInteractionInProgress) return;

        IsInteractionInProgress = false;
        OnEndInteraction();
        Interactor = null;
    }

    /// <summary>
    /// Called when the interact key is released.
    /// </summary>
    /// <remarks>
    /// Guaranteed to follow one call to <see cref="Interact(Employee)"/>.
    /// </remarks>
    protected virtual void OnEndInteraction() { }
}
