using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class NeonSign : MonoBehaviour
{
    [SerializeField] Renderer[] signParts;
    [SerializeField] EventReference hum;
    [SerializeField, Min(0.01f)] float fadeInSpeed = 1f;
    [SerializeField, Min(0.01f)] float fadeOutSpeed = 1f;
    [SerializeField] Ease inEase = Ease.OutBack;
    [SerializeField] Ease outEase = Ease.OutBounce;

    Color[] onColors;

    EventInstance humInstance;

    float currentIntensity;
    Tween fadeTween;

    const string Emission = "_EmissionColor";

    [ContextMenu("Turn On")]
    public void Set() => Set(true);

    void Awake()
    {
        onColors = new Color[signParts.Length];

        for (var i = 0; i < signParts.Length; i++)
        {
            var part = signParts[i];
            onColors[i] = part.material.GetColor(Emission);
        }

        Set(false);

        humInstance = RuntimeManager.CreateInstance(hum);
        RuntimeManager.AttachInstanceToGameObject(humInstance, transform);
    }

    public void Set(bool on)
    {
        var speed = on ? fadeInSpeed : fadeOutSpeed;
        var goal = on ? 1 : 0;
        var ease = on ? inEase : outEase;
        fadeTween?.Kill();
        fadeTween = DOTween.To(() => currentIntensity, SetIntensity, goal, speed)
            .SetSpeedBased()
            .SetEase(ease);

        if (on)
            humInstance.start();
        else
            humInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void SetIntensity(float intensity)
    {
        for (var i = 0; i < signParts.Length; i++)
        {
            var part = signParts[i];
            var color = onColors[i];

            color *= intensity;

            part.material.SetColor(Emission, color);
        }

        currentIntensity = intensity;
    }

    void OnDestroy()
    {
        humInstance.release();
    }
}
