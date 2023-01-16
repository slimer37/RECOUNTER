using DG.Tweening;
using UnityEngine;

public class TestInteractable : Interactable
{
    Tween tween;

    void Awake()
    {
        tween = transform.DOScale(Vector3.one * 1.25f, 0.3f)
            .SetEase(Ease.OutBack)
            .Pause()
            .SetAutoKill(false);
    }

    protected override void OnInteract(Employee e) => tween.PlayForward();
    protected override void OnEndInteraction() => tween.PlayBackwards();
}
