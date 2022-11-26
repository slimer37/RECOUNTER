using UnityEngine;

[System.Serializable]
public class AudioClipGroup
{
    enum SelectionMode
    {
        Random,
        PingPong,
        Sequential
    }

    [SerializeField] AudioClip[] clips;
    [SerializeField] SelectionMode mode;
    [SerializeField, Range(0, 1)] float volume = 1;
    [SerializeField, Range(0, 1)] float volumeVariation;
    [SerializeField, Range(-3, 3)] float pitch = 1;
    [SerializeField, Range(0, 6)] float pitchVariation;

    int i;

    public AudioClip NextClip()
    {
        return mode switch
        {
            SelectionMode.Random => clips[Random.Range(0, clips.Length)],
            SelectionMode.PingPong => clips[(int)Mathf.PingPong(i++, clips.Length)],
            SelectionMode.Sequential => clips[i++ % clips.Length],
            _ => throw new System.Exception("Invalid selection mode.")
        };
    }

    public void PlayOneShot(AudioSource source)
    {
        var volVaried = volume + Random.Range(-volumeVariation, volumeVariation);
        var pitchVaried = pitch + Random.Range(-pitchVariation, pitchVariation);
        source.pitch = pitchVaried;
        source.PlayOneShot(NextClip(), volVaried);
    }
}