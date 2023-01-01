using DG.Tweening;
using System;
using UnityEngine;

public class BoxFlaps : Interactable
{
    [Serializable]
    struct Flap
    {
        [SerializeField] Transform transform;
        [SerializeField] Vector3 localRotationAmount;

        public Tween Animate(bool open, float duration, Ease ease) =>
            transform.DOLocalRotate((open ? 1 : -1) * localRotationAmount, duration, RotateMode.LocalAxisAdd).SetRelative().SetEase(ease);
    }

    [SerializeField] Flap[] upperFlaps;
    [SerializeField] float upperFlapDuration;
    [SerializeField] Ease upperFlapEase;
    [SerializeField] Flap[] lowerFlaps;
    [SerializeField] float lowerFlapDuration;
    [SerializeField] Ease lowerFlapEase;
    [SerializeField] float timeToNextFlapSet;

    Sequence sequence;

    public bool FlapsAreOpen { get; private set; }
    public bool Animating => sequence.IsActive() && sequence.IsPlaying();

    protected override bool CanInteract(Employee e) => !Animating;

    public override HudInfo GetHudInfo(Employee e) => CanInteract(e)
        ? new()
        {
            icon = Icon.Hand,
            text = (FlapsAreOpen ? "Close" : "Open") + " flaps"
        }
        : BlankHud;

    protected override void OnInteract(Employee e)
    {
        var open = !FlapsAreOpen;
        FlapsAreOpen = open;

        sequence = DOTween.Sequence();

        foreach (var flap in upperFlaps)
        {
            sequence.Insert(open ? 0 : timeToNextFlapSet, flap.Animate(open, upperFlapDuration, upperFlapEase));
        }

        foreach (var flap in lowerFlaps)
        {
            sequence.Insert(open ? timeToNextFlapSet : 0, flap.Animate(open, lowerFlapDuration, lowerFlapEase));
        }
    }

    void OnDestroy() => sequence?.Kill();
}
