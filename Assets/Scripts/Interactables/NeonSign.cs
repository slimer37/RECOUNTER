using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class NeonSign : MonoBehaviour
{
    [SerializeField] Renderer[] signParts;
    [SerializeField] EventReference hum;
    [SerializeField, Min(0.01f)] float fadeInSpeed = 1f;
    [SerializeField, Min(0.01f)] float fadeOutSpeed = 1f;

    Color[] onColors;

    float fadeTime;

    bool isOn;

    EventInstance humInstance;

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

    void Update()
    {
        if (isOn && fadeTime > 1) return;
        if (!isOn && fadeTime < 0) return;

        fadeTime += Time.deltaTime * (isOn ? fadeInSpeed : -fadeOutSpeed);

        SetIntensity(fadeTime);
    }

    public void Set(bool on)
    {
        isOn = on;

        if (on)
            humInstance.start();
        else
            humInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void SetIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        for (var i = 0; i < signParts.Length; i++)
        {
            var part = signParts[i];
            var color = onColors[i];

            color *= intensity;

            part.material.SetColor(Emission, color);
        }
    }

    void OnDestroy()
    {
        humInstance.release();
    }
}
