using System;
using UnityEngine;

public class PowerInlet : Interactable
{
    [SerializeField] float requirement;
    [SerializeField] Vector3 plugPoint;

    public event Action Powered;
    public event Action Depowered;

    Wire wire;

    public bool IsPowered { get; private set; }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e)
    {
        return wire ? wire == WireManager.ActiveWire : !WireManager.ActiveWire;
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = Icon.Hand,
                text = wire ? "Put Away Wire" : "Connect Wire"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        if (wire)
        {
            WireManager.ReleaseWire(wire);
            WireManager.ClearActiveWire();
            wire = null;
        }
        else
        {
            wire = WireManager.GetWire();

            wire.SetStart(
                this,
                transform.TransformPoint(plugPoint),
                transform.forward,
                Camera.main.transform,
                Vector3.forward);

            wire.Connected += OnPower;
            wire.Disconnected += OnDepower;
        }
    }

    void OnDepower(PowerInlet inlet, PowerOutlet outlet)
    {
        IsPowered = false;
        Depowered?.Invoke();
    }

    void OnPower(PowerInlet inlet, PowerOutlet outlet)
    {
        IsPowered = true;
        Powered?.Invoke();
    }
}
