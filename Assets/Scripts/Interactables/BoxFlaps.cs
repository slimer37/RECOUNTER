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
    [SerializeField] Ease upperFlapOpenEase;
    [SerializeField] Ease upperFlapCloseEase;
    [SerializeField] Flap[] lowerFlaps;
    [SerializeField] float lowerFlapDuration;
    [SerializeField] Ease lowerFlapOpenEase;
    [SerializeField] Ease lowerFlapCloseEase;
    [SerializeField] float timeToNextFlapSet;

    Sequence sequence;

    public bool FlapsAreOpen { get; private set; }
    public bool Animating => sequence.IsActive() && sequence.IsPlaying();

    protected override bool CanInteract(Employee e) => !Animating;

    protected override HudInfo FormHud(Employee e) => new()
    {
        icon = Icon.Hand,
        text = (FlapsAreOpen ? "Close" : "Open") + " flaps"
    };

    protected override void OnInteract(Employee e)
    {
        var open = !FlapsAreOpen;
        FlapsAreOpen = open;

        sequence = DOTween.Sequence();

        foreach (var flap in upperFlaps)
        {
            sequence.Insert(
                open ? 0 : timeToNextFlapSet,
                flap.Animate(open, upperFlapDuration, open ? upperFlapOpenEase : upperFlapCloseEase));
        }

        foreach (var flap in lowerFlaps)
        {
            sequence.Insert(
                open ? timeToNextFlapSet : 0,
                flap.Animate(open, lowerFlapDuration, open ? lowerFlapOpenEase : lowerFlapCloseEase));
        }
    }

    void OnDestroy() => sequence?.Kill();
}
