using UnityEngine;

namespace Recounter
{
    /// <summary>
    /// Convenience class for common hold interaction.
    /// Defines a hold interaction as one held for a specified duration and executed exactly upon reaching that duration,
    /// and a short interaction as one held for less than that duration.
    /// </summary>
    public abstract class HoldInteractable : Interactable
    {
        protected virtual float RequiredHoldTime => 0.2f;

        protected float ElapsedHoldTime { get; private set; }
        protected bool HoldInteractionInProgress { get; private set; }

        protected virtual Icon PressIcon => Icon.Access;
        protected abstract string PressAction { get; }
        protected virtual Icon HoldIcon => Icon.Access;
        protected abstract string HoldAction { get; }

        protected override HudInfo FormHud(Employee e)
        {
            var holding = HoldInteractionInProgress;

            return new()
            {
                icon = holding ? HoldIcon : PressIcon,
                text = $"<sprite name=\"Press\"> {PressAction}\n<sprite name=\"Hold\"> {HoldAction}",
                fill = holding ? (ElapsedHoldTime / RequiredHoldTime) : null
            };
        }

        protected sealed override void OnInteract(Employee e)
        {
            ElapsedHoldTime = 0;
            HoldInteractionInProgress = true;
            OnStartHold();
        }

        protected virtual void OnStartHold() { }

        void HoldInteract()
        {
            OnHoldInteract();
            HoldInteractionInProgress = false;
        }

        protected abstract void OnHoldInteract();

        protected sealed override void OnEndInteraction()
        {
            OnReleaseHold();

            if (!HoldInteractionInProgress) return;

            HoldInteractionInProgress = false;

            OnShortInteract();
        }

        protected virtual void OnReleaseHold() { }

        protected abstract void OnShortInteract();

        void Update()
        {
            if (!HoldInteractionInProgress) return;

            ElapsedHoldTime += Time.deltaTime;

            var normalizedTime = ElapsedHoldTime / RequiredHoldTime;

            PerFrameHeld(normalizedTime);

            if (normalizedTime >= 1f) HoldInteract();
        }

        public virtual void PerFrameHeld(float normalizedTime) { }
    }
}
