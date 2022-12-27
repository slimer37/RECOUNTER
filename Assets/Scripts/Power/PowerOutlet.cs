using UnityEngine;

public class PowerOutlet : Interactable
{
    [SerializeField] Vector3 plugPoint;
    [SerializeField] ParticleSystem sparks;

    PowerInlet inlet;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(plugPoint, 0.05f);
    }

    protected override bool CanInteract(Employee e)
    {
        return !inlet && WireManager.ActiveWire;
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
        var wire = WireManager.ActiveWire;
        wire.Connected += FinishConnection;
        wire.Connect(this, transform.TransformPoint(plugPoint), transform.forward);
        
    }

    void FinishConnection(PowerInlet inlet, PowerOutlet outlet)
    {
        this.inlet = inlet;
        sparks.Play();
    }
}
