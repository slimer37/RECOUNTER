using UnityEngine;

public class NeonSign : MonoBehaviour
{
    [SerializeField] Renderer[] signParts;
    [SerializeField] float offIntensity;

    Color[] onColors;

    const string Emission = "_EmissionColor";

    [ContextMenu("Turn On")]
    public void Set() => Set(true);

    void Awake()
    {
        onColors = new Color[signParts.Length];

        for (var i = 0; i < signParts.Length; i++)
        {
            var part = signParts[i];
            onColors[i] = part.material.GetColor(Emission);
        }

        Set(false);
    }

    public void Set(bool on)
    {
        for (var i = 0; i < signParts.Length; i++)
        {
            var part = signParts[i];
            var color = onColors[i];

            if (!on)
                color *= offIntensity;

            part.material.SetColor(Emission, color);
        }
    }
}
