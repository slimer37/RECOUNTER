using System;
using UnityEngine;

public class PowerInlet : Interactable
{
    [SerializeField] float requirement;
    [SerializeField] Vector3 plugPoint;

    public event Action Powered;
    public event Action Depowered;

    PowerOutlet outlet;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.1f);
    }

    protected override bool CanInteract(Employee e)
    {
        return !outlet && !WireManager.ActiveWire;
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = Icon.Hand,
                text = "Connect Wire"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        var wire = WireManager.GetWire();

        wire.SetStart(
            this,
            transform.TransformPoint(plugPoint),
            transform.forward,
            e.transform,
            Vector3.forward + Vector3.up);

        wire.Connected += OnPower;
    }

    void OnPower(PowerInlet inlet, PowerOutlet outlet)
    {
        this.outlet = outlet;
        Powered?.Invoke();
    }
}
