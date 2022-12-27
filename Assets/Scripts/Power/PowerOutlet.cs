using UnityEngine;

public class PowerOutlet : Interactable
{
    [SerializeField] Vector3 plugPoint;
    [SerializeField] ParticleSystem sparks;

    PowerInlet inlet;
    Wire wire;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e)
    {
        return !wire || !wire.IsConnecting;
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = Icon.Hand,
                text = wire ? "Unplug" : "Plug"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        if (wire)
        {
            wire.Disconnect(Camera.main.transform, Vector3.forward);
            sparks.Play();
            wire = null;
        }
        else
        {
            wire = WireManager.ActiveWire;
            wire.Connected += FinishConnection;
            wire.Connect(this, transform.TransformPoint(plugPoint), transform.forward);
        }
    }

    void FinishConnection(PowerInlet inlet, PowerOutlet outlet)
    {
        this.inlet = inlet;
        sparks.Play();
    }
}
