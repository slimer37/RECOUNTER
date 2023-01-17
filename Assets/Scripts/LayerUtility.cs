using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LayerUtility
{
	static Dictionary<GameObject, int[]> _originalLayers = new();

	[RuntimeInitializeOnLoadMethod]
	static void Init() => SceneManager.sceneLoaded += (_, _) => _originalLayers.Clear();

	public static void SetHierarchyLayersWithoutRestore(this GameObject gameObject, int layer)
	{
        var hierarchy = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var t in hierarchy)
		{
			t.gameObject.layer = layer;
		}
    }

    public static bool HierarchyLayersAreSet(this GameObject gameObject) => _originalLayers.ContainsKey(gameObject);

    public static void SetHierarchyLayers(this GameObject gameObject, int layer)
	{
		if (gameObject.HierarchyLayersAreSet())
			throw new InvalidOperationException($"Cannot set layers for {gameObject.name}. Restore its layers before setting again.");

		var hierarchy = gameObject.GetComponentsInChildren<Transform>(true);
		var recordedLayers = new int[hierarchy.Length];

		for (int i = 0; i < hierarchy.Length; i++)
		{
			var obj = hierarchy[i].gameObject;
            recordedLayers[i] = obj.layer;
			obj.layer = layer;
		}

		_originalLayers[gameObject] = recordedLayers;
	}

	public static void RestoreHierarchyLayers(this GameObject gameObject)
	{
		if (!_originalLayers.TryGetValue(gameObject, out var recordedLayers))
			throw new KeyNotFoundException($"The original layers were not found for the hierarchy of {gameObject.name}.");

        var hierarchy = gameObject.GetComponentsInChildren<Transform>();

		if (hierarchy.Length != recordedLayers.Length)
			throw new InvalidOperationException($"The hierarchy of {gameObject} changed since its layer was set.");

        for (int i = 0; i < hierarchy.Length; i++)
        {
            hierarchy[i].gameObject.layer = recordedLayers[i];
        }

		_originalLayers.Remove(gameObject);
    }
}
