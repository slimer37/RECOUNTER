using DG.Tweening;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] float fadeDuration;
    [SerializeField] float duration;
    [SerializeField] CanvasGroup background;
    [SerializeField] CanvasGroup group;
    [SerializeField] Canvas canvas;

    void Awake()
    {
        canvas.enabled = true;

        background.alpha = 1;
        group.alpha = 0;

        DOTween.Sequence()
            .Append(group.DOFade(1, fadeDuration))
            .AppendInterval(duration)
            .Append(group.DOFade(0, fadeDuration))
            .Join(background.DOFade(0, fadeDuration))
            .OnComplete(() => canvas.enabled = false);
    }
}
