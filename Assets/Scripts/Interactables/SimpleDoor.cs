using DG.Tweening;
using UnityEngine;

public class SimpleDoor : Interactable
{
    [SerializeField] Transform door;
    [SerializeField] Vector3 localOpenRotation;
    [SerializeField] Ease openEase;
    [SerializeField] Ease closeEase;
    [SerializeField] float tweenSpeed;
    [SerializeField] string label;

    bool isOpen;

    Vector3 localClosedRotation;

    Tween tween;

    public override HudInfo GetHudInfo(Employee e)
    {
        return new()
        {
            icon = Icon.Hand,
            text = $"Open {label}",
        };
    }

    void Awake()
    {
        localClosedRotation = door.localEulerAngles;
    }

    protected override void OnInteract(Employee e)
    {
        tween?.Kill();

        isOpen = !isOpen;
        tween = door.DOLocalRotate(isOpen ? localOpenRotation : localClosedRotation, tweenSpeed)
            .SetSpeedBased()
            .SetEase(isOpen ? openEase : closeEase);
    }
}
