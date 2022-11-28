using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class AudioMixerPref : MonoBehaviour
{
    [SerializeField] string parameter;

    public void OnVolumeChanged(float v)
    {
        VolumeInitializer.SetMixerParam(parameter, v);
    }
}
