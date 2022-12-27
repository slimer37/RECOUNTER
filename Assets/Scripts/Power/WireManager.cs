using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public static class WireManager
{
    static ObjectPool<Wire> wires;

    static Wire wirePrefab;

    public const string prefabKey = "Wire";

    public static Wire ActiveWire { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    static async void Init()
    {
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        await loadHandle.Task;
        wirePrefab = loadHandle.Result.GetComponent<Wire>();

        if (!wirePrefab)
            throw new NullReferenceException($"No valid addressable asset found with name \"{prefabKey}\"");

        wires = new ObjectPool<Wire>(Create, Get, Release);
    }

    static Wire Create() => UnityEngine.Object.Instantiate(wirePrefab);

    static void Get(Wire wire) => wire.enabled = true;

    static void Release(Wire wire) => wire.enabled = false;

    public static Wire GetWire()
    {
        ActiveWire = wires.Get();
        ActiveWire.Connected += (_, _) => ActiveWire = null;
        return ActiveWire;
    }
}
