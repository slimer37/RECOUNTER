using DG.Tweening;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] RectTransform credits;
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup group;
    [SerializeField, Min(0.01f)] float speed = 5;
    [SerializeField] float screenHeight = 1080;
    [SerializeField, Min(0.01f)] float fadeTime = 2;

    Controls controls;

    Tween creditsRoll;

    void Awake()
    {
        canvas.enabled = false;

        controls = new Controls();
        controls.Enable();
        controls.Menu.Exit.performed += _ => Hide();

        group.alpha = 0;

        var creditsHeight = credits.rect.height;
        var top = creditsHeight / 2 + screenHeight / 2;
        var bottom = -top;

        credits.localPosition = Vector3.up * bottom;

        creditsRoll = credits.DOLocalMoveY(top, speed)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .SetDelay(fadeTime)
            .Pause()
            .SetAutoKill(false)
            .OnComplete(Hide);
    }

    void OnDestroy()
    {
        creditsRoll.Kill();
        controls.Dispose();
    }

    void End()
    {
        canvas.enabled = false;
        creditsRoll.Pause();
    }

    public void Roll()
    {
        group.blocksRaycasts = true;

        canvas.enabled = true;
        group.DOKill();
        group.DOFade(1, fadeTime);
        creditsRoll.Restart();
    }

    void Hide()
    {
        if (!canvas.enabled) return;

        group.blocksRaycasts = false;

        group.DOKill();
        group.DOFade(0, fadeTime).OnComplete(End);
    }
}
