using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [Serializable]
    struct Splash
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] float duration;
        [SerializeField] float fadeDuration;

        public void AppendTo(Sequence sequence, bool last)
        {
            group.alpha = 0;

            sequence
                .Append(group.DOFade(1, fadeDuration))
                .AppendInterval(duration);

            if (last) return;

            sequence.Append(group.DOFade(0, fadeDuration));
        }

        public Tween FadeOut() => group.DOFade(0, fadeDuration);
    }

    [SerializeField] AssetReference titleScreenReference;

    [Header("Splash Screens")]
    [SerializeField] float finalFadeOut;
    [SerializeField] CanvasGroup background;
    [SerializeField] Splash[] splashes;

    [Header("Components")]
    [SerializeField] Canvas canvas;
    [SerializeField] GraphicRaycaster raycaster;

    AsyncOperationHandle<SceneInstance> sceneHandle;

    void Awake()
    {
        sceneHandle = titleScreenReference.LoadSceneAsync(LoadSceneMode.Additive);

        canvas.enabled = true;

        background.alpha = 1;

        var sequence = DOTween.Sequence(this);

        for (var i = 0; i < splashes.Length; i++)
        {
            splashes[i].AppendTo(sequence, i == splashes.Length - 1);
        }

        sequence.AppendCallback(OnLogosFinished);
    }

    async void OnLogosFinished()
    {
        raycaster.enabled = false;

        await sceneHandle.Task;

        DOTween.Sequence()
            .Append(splashes[^1].FadeOut())
            .Join(background.DOFade(0, finalFadeOut))
            .AppendCallback(OnSplashScreenFinished);
    }

    void OnSplashScreenFinished()
    {
        canvas.enabled = false;
    }
}
