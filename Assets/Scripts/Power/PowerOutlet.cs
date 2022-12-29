using UnityEngine;

public class PowerOutlet : Interactable
{
    [SerializeField] Vector3 plugPoint;
    [SerializeField] string label = "Outlet";

    [Header("Particles")]
    [SerializeField] ParticleSystem sparks;

    PowerInlet inlet;
    Wire wire;

    public bool IsPluggedIn => wire;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e)
    {
        return IsPluggedIn ? !wire.IsAnimating : WireManager.ActiveWire;
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = IsPluggedIn ? Icon.Unplug : Icon.Outlet,
                text = IsPluggedIn ? $"Unplug From {label}" : $"Plug Into {label}"
            }
            : BlankHud;
    }

    protected override void OnInteract(Employee e)
    {
        if (IsPluggedIn)
        {
            wire.Disconnect(Camera.main.transform, Vector3.forward);
            wire = null;

            Spark();
        }
        else
        {
            wire = WireManager.ActiveWire;
            wire.Connected += FinishConnection;
            wire.Connect(this, transform.TransformPoint(plugPoint), transform.forward, transform.up);
        }
    }

    void FinishConnection(PowerInlet inlet, PowerOutlet outlet)
    {
        this.inlet = inlet;
        wire.Connected -= FinishConnection;

        Spark();
    }

    void Spark()
    {
        if (sparks)
        {
            sparks.transform.position = transform.TransformPoint(plugPoint);
            sparks.Play();
        }
    }
}
