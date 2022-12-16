using DG.Tweening;
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

        public void AppendTo(Sequence sequence)
        {
            group.alpha = 0;

            sequence
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
            splash.AppendTo(sequence);
        }

        sequence
            .AppendCallback(OnLogosFinished)
            .Join(background.DOFade(0, finalFadeOut))
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
