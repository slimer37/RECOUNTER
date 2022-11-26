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
}