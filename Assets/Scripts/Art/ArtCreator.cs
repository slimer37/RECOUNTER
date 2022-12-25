using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ArtCreator
{
    public class ArtSession
    {
        public event Action<Texture> Completed;

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
    }

    static GameObject artCreatorPrefab;

    static GameObject currentArtCreator;

    static ArtSession currentArtSession;

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
        currentArtSession.Complete(result);
    }

    public static ArtSession BeginSession()
    {
        currentArtCreator = UnityEngine.Object.Instantiate(artCreatorPrefab);
        return currentArtSession = new ArtSession();
    }
}
