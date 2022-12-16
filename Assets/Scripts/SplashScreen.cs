using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [Serializable]
    struct Splash
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] float duration;
        [SerializeField] float fadeDuration;

        public Tween GetTween(object target)
        {
            group.alpha = 0;

            return DOTween.Sequence(target)
            .Append(group.DOFade(1, fadeDuration))
            .AppendInterval(duration)
            .Append(group.DOFade(0, fadeDuration));
        }
    }

    [SerializeField] float finalFadeOut;
    [SerializeField] CanvasGroup background;
    [SerializeField] Splash[] splashes;

    [Header("Components")]
    [SerializeField] Canvas canvas;
    [SerializeField] GraphicRaycaster raycaster;

    void Awake()
    {
        canvas.enabled = true;

        background.alpha = 1;

        var sequence = DOTween.Sequence(this);

        foreach (var splash in splashes)
        {
            sequence.Append(splash.GetTween(this));
        }

        sequence
            .AppendCallback(OnLogosFinished)
            .Append(background.DOFade(0, finalFadeOut))
            .AppendCallback(OnSplashScreenFinished);
    }

    void OnLogosFinished()
    {
        raycaster.enabled = false;
    }

    void OnSplashScreenFinished()
    {
        canvas.enabled = false;
    }
}
