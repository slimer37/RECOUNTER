using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class PowerInlet : Interactable
{
    [SerializeField] float requirement;
    [SerializeField] Transform wireAttach;

    [Foldout("Events")] public UnityEvent Powered;
    [Foldout("Events")] public UnityEvent Depowered;

    [Foldout("Events")] public UnityEvent<bool> StateChanged;

    Wire wire;

    public bool IsPluggedIn { get; private set; }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(wireAttach.position, 0.05f);
    }

    protected override bool CanInteract(Employee e) =>
        wire ? e.LeftHand == wire.Holder : !e.LeftHand.IsFull;

    protected override HudInfo FormHud(Employee e) => new()
    {
        icon = wire ? Icon.StashPlug : Icon.Plug,
        text = wire ? "Stash Plug" : "Grab Plug"
    };

    protected override void OnInteract(Employee e)
    {
        if (wire)
        {
            e.LeftHand.Clear();
            StashWire();
        }
        else
        {
            wire = WireManager.GetWire();

            wire.SetStart(
                this,
                wireAttach.position,
                wireAttach.forward,
                wireAttach.up,
                e.LeftHand);

            wire.Connected += OnPower;
            wire.Disconnected += OnDepower;
        }
    }

    void StashWire()
    {
        WireManager.ReleaseWire(wire);

        wire.Connected -= OnPower;
        wire.Disconnected -= OnDepower;

        wire = null;
        IsPluggedIn = false;
    }

    void OnDepower(PowerInlet inlet, PowerOutlet outlet)
    {
        Depowered?.Invoke();
        StateChanged?.Invoke(false);
        IsPluggedIn = false;
    }

    void OnPower(PowerInlet inlet, PowerOutlet outlet)
    {
        Powered?.Invoke();
        StateChanged?.Invoke(true);
        IsPluggedIn = true;
    }
}
