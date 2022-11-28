using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] RectTransform credits;
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup group;
    [SerializeField, Min(0.01f)] float speed = 5;
    [SerializeField, Min(0), MaxValue(1)] float pivotGoal;
    [SerializeField, Min(0.01f)] float fadeTime = 2;

    Controls.MenuActions controls;

    Tween creditsScroll;

    void Awake()
    {
        canvas.enabled = false;

        controls = new Controls().Menu;
        controls.Enable();
        controls.Exit.performed += _ => Hide();

        group.alpha = 0;

        var anchor = new Vector2(0.5f, 1 - pivotGoal);

        creditsScroll = DOTween.Sequence()
            .AppendInterval(fadeTime)
            .Append(credits.DOPivotY(pivotGoal, credits.rect.height / speed).SetEase(Ease.Linear))
            .Join(credits.DOAnchorMax(anchor, credits.rect.height / speed).SetEase(Ease.Linear))
            .Join(credits.DOAnchorMax(anchor, credits.rect.height / speed).SetEase(Ease.Linear))
            .Pause()
            .SetAutoKill(false)
            .OnComplete(Hide);
    }

    void End()
    {
        canvas.enabled = false;
        creditsScroll.Pause();
    }

    public void Show()
    {
        canvas.enabled = true;
        group.DOKill();
        group.DOFade(1, fadeTime);
        creditsScroll.Restart();
    }

    void Hide()
    {
        group.DOKill();
        group.DOFade(0, fadeTime).OnComplete(End);
    }
}
