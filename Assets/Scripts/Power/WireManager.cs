﻿using System;
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

    static Wire Create()
    {
        var wire = UnityEngine.Object.Instantiate(wirePrefab);
        wire.gameObject.SetActive(false);
        return wire;
    }

    static void Get(Wire wire)
    {
        wire.gameObject.SetActive(true);
    }

    static void Release(Wire wire)
    {
        wire.gameObject.SetActive(false);
    }

    public static Wire GetWire()
    {
        SetActiveWire(wires.Get());
        return ActiveWire;
    }

    public static void SetActiveWire(Wire wire)
    {
        if (ActiveWire)
            ActiveWire.Connected -= ClearActiveWire;

        ActiveWire = wire;
        ActiveWire.Connected += ClearActiveWire;
    }

    static void ClearActiveWire(PowerInlet inlet, PowerOutlet outlet)
    {
        if (ActiveWire)
            ActiveWire.Connected -= ClearActiveWire;

        ActiveWire = null;
    }

    public static void ClearActiveWire()
    {
        ClearActiveWire(null, null);
    }

    public static void ReleaseWire(Wire wire) => wires.Release(wire);
}