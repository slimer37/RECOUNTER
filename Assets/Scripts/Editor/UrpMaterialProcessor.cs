using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

public class UrpMaterialProcessor : AssetPostprocessor
{
    readonly Dictionary<string, string> floatDefinitions = new()
    {
        { "ReflectionFactor", "_Metallic" }
    };

    public override int GetPostprocessOrder() => 10;

    void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] animations)
    {
        foreach (var key in floatDefinitions.Keys)
        {
            if (!description.TryGetProperty(key, out float value))
                throw new Exception($"Could not find key {key} in material description.");
                
            material.SetFloat(floatDefinitions[key], value);
        }
            
        if (description.TryGetProperty("DiffuseColor", out TexturePropertyDescription tex))
            material.mainTextureScale = new Vector2(1 / tex.scale.x, 1 / tex.scale.y);
    }
        
    void LogFloatProperties(MaterialDescription description)
    {
        var propertyNames = new List<string>();
        description.GetFloatPropertyNames(propertyNames);
            
        for (var i = 0; i < propertyNames.Count; i++)
        {
            description.TryGetProperty(propertyNames[i], out float value);
            propertyNames[i] += " " + value;
        }
            
        Debug.Log(description.materialName + "\n" + string.Join('\n', propertyNames.ToArray()));
    }
}