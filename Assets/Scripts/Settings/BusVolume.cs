using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class BusVolume : MonoBehaviour
{
    [SerializeField] string busPath = "bus:/";
    [SerializeField] string volumePrefKey = "MasterVol";
    [SerializeField] EventReference soundUpdateEvent;

    Bus bus;

    void Awake()
    {
        bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(PlayerPrefs.GetFloat(volumePrefKey));

        PrefManager.OnFloatPrefChanged += OnFloatPrefChanged;
    }

    void OnFloatPrefChanged(string key, float value)
    {
        if (key != volumePrefKey) return;

        bus.setVolume(value);

        if (soundUpdateEvent.IsNull) return;
        
        RuntimeManager.PlayOneShot(soundUpdateEvent);
    }
}
