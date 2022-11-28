using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class VolumeInitializer : MonoBehaviour
{
    static readonly string[] MixerParams = { "MasterVol" };

    static AudioMixer mixer;

    [RuntimeInitializeOnLoadMethod]
    static void InitVolume()
    {
        mixer = Addressables.LoadAssetAsync<AudioMixer>("Audio Mixer").WaitForCompletion();

        new GameObject("Volume Initializer").AddComponent<VolumeInitializer>();
    }

    // Workaround for editor, since volume does not like getting set in RuntimeInit functions.
    void Start()
    {
        foreach (var p in MixerParams)
        {
            if (!PlayerPrefs.HasKey(p)) continue;

            mixer.SetFloat(p, ConvertLinearVolume(PlayerPrefs.GetFloat(p)));
        }

        Destroy(gameObject);
    }

    static float ConvertLinearVolume(float v)
    {
        if (v is < 1e-4f or > 1)
            throw new ArgumentOutOfRangeException("Volume must be in the range 1e-4 to 1.");

        return Mathf.Log10(v) * 20;
    }

    public static void SetMixerParam(string param, float value)
    {
        if (!MixerParams.Contains(param))
            throw new ArgumentException($"Provided parameter is not one of {nameof(MixerParams)}.", nameof(param));

        mixer.SetFloat(param, ConvertLinearVolume(value));
    }
}
