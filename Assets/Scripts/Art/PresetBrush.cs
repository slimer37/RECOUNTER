using UnityEngine;

public class PresetBrush : Brush
{
    [SerializeField] Color color;
    [SerializeField] float radius;

    protected override void Awake()
    {
        base.Awake();

        Color = color;
        Radius = radius;
    }
}