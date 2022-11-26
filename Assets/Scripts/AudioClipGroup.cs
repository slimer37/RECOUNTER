using UnityEngine;

[System.Serializable]
public class AudioClipGroup
{
    enum SelectionMode
    {
        Random,
        RandomNoImmediateRepeat,
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
        switch (mode)
        {
            case SelectionMode.Random:
                return clips[Random.Range(0, clips.Length)];
            case SelectionMode.RandomNoImmediateRepeat:
                var j = Random.Range(0, clips.Length - 1);
                if (i == j) j++;
                i = j;
                return clips[j];
            case SelectionMode.Sequential:
                return clips[i++ % clips.Length];
            default:
                throw new System.Exception("Invalid selection mode.");
        }
    }

    public void PlayOneShot(AudioSource source)
    {
        var volVaried = volume + Random.Range(-volumeVariation, volumeVariation);
        var pitchVaried = pitch + Random.Range(-pitchVariation, pitchVariation);
        source.pitch = pitchVaried;
        source.PlayOneShot(NextClip(), volVaried);
    }
}