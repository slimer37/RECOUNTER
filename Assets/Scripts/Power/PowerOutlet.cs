using UnityEngine;

public class PowerOutlet : Interactable
{
    [SerializeField] Vector3 plugPoint;
    [SerializeField] string label = "Outlet";

    PowerInlet inlet;
    Wire wire;

    public bool IsPluggedIn => wire;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e) =>
        !IsPluggedIn && e.LeftHand.ContainsParentComponent<Wire>(out _);

    protected override HudInfo FormHud(Employee e) => new()
    {
        icon = Icon.Outlet,
        text = $"Plug Into {label}"
    };

    protected override void OnInteract(Employee e)
    {
        wire = e.LeftHand.HeldObject.GetComponentInParent<Wire>();
        wire.Connected += FinishConnection;
        wire.Connect(this, transform.TransformPoint(plugPoint), transform.forward, transform.up);
    }

    void FinishConnection(PowerInlet inlet, PowerOutlet outlet)
    {
        this.inlet = inlet;
        wire.Connected -= FinishConnection;
        wire.Disconnected += Unplug;
    }

    void Unplug(PowerInlet inlet, PowerOutlet outlet)
    {
        wire.Disconnected -= Unplug;
        wire = null;
    }
}
