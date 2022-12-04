using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class VolumePref : MonoBehaviour
{
    static Bus masterBus;

    [RuntimeInitializeOnLoadMethod]
    static void InitVolume()
    {
        masterBus = RuntimeManager.GetBus("bus:/");
        masterBus.setVolume(PlayerPrefs.GetFloat("MasterVol"));
    }

    public void OnVolumeChanged(float v)
    {
        masterBus.setVolume(v);
    }
}
