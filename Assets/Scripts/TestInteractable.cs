using DG.Tweening;
using UnityEngine;

public class TestInteractable : Interactable
{
    Tween tween;

    void Awake()
    {
        tween = transform.DOPunchScale(Vector3.one * 0.25f, 0.3f)
            .Pause().SetAutoKill(false);
    }

    public override void Interact() => tween.Restart();
}
