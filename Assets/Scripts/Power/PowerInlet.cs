using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class PowerInlet : Interactable
{
    [SerializeField] float requirement;
    [SerializeField] Vector3 plugPoint;

    [Foldout("Events")] public UnityEvent Powered;
    [Foldout("Events")] public UnityEvent Depowered;

    [Foldout("Events")] public UnityEvent<bool> StateChanged;

    Wire wire;

    public bool IsPluggedIn => wire;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e)
    {
        return IsPluggedIn ? wire == WireManager.ActiveWire : !WireManager.ActiveWire;
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = IsPluggedIn ? Icon.StashPlug : Icon.Plug,
                text = IsPluggedIn ? "Stash Plug" : "Grab Plug"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        if (IsPluggedIn)
        {
            StashWire();
        }
        else
        {
            wire = WireManager.GetWire();

            wire.SetStart(
                this,
                transform.TransformPoint(plugPoint),
                transform.forward,
                transform.up,
                Camera.main.transform,
                Vector3.forward);

            wire.Connected += OnPower;
            wire.Disconnected += OnDepower;
        }
    }

    void StashWire()
    {
        WireManager.ReleaseWire(wire);
        WireManager.ClearActiveWire();

        wire.Connected -= OnPower;
        wire.Disconnected -= OnDepower;

        wire = null;
    }

    void OnDepower(PowerInlet inlet, PowerOutlet outlet)
    {
        Depowered?.Invoke();
        StateChanged?.Invoke(false);
    }

    void OnPower(PowerInlet inlet, PowerOutlet outlet)
    {
        Powered?.Invoke();
        StateChanged?.Invoke(true);
    }
}
