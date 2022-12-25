using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ArtCreator
{
    public class ArtSession
    {
        public event Action<Texture> Completed;

        public readonly Texture Initial;

        public Texture Result { get; private set; }
        public bool IsComplete { get; private set; }

        public void Complete(Texture result)
        {
            if (IsComplete)
                throw new InvalidOperationException("Art session already complete.");

            IsComplete = true;
            Result = result;
            Completed?.Invoke(result);
        }

        public ArtSession(Texture initialTexture)
        {
            Initial = initialTexture;
        }
    }

    static GameObject artCreatorPrefab;

    static GameObject currentArtCreator;

    public static ArtSession CurrentArtSession { get; private set; }

    public static bool SessionInProgress => CurrentArtSession != null && !CurrentArtSession.IsComplete;

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        Addressables.LoadAssetAsync<GameObject>("ArtCreator").Completed += h =>
        {
            artCreatorPrefab = h.Result;
        };
    }

    public static void Complete(Texture result)
    {
        UnityEngine.Object.Destroy(currentArtCreator);
        CurrentArtSession.Complete(result);
    }

    public static ArtSession BeginSession(Texture initialTexture = null)
    {
        if (SessionInProgress)
            throw new InvalidOperationException("An art session is still ongoing.");

        CurrentArtSession = new ArtSession(initialTexture);
        currentArtCreator = UnityEngine.Object.Instantiate(artCreatorPrefab);
        return CurrentArtSession;
    }
}
