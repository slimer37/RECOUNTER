using UnityEngine;

namespace Recounter.Art
{
    public class Eraser : ComputeShaderBrush
    {
        [SerializeField] Artboard artboard;
        [SerializeField] float startingRadius;

        new public float Radius
        {
            get => base.Radius;
            set => base.Radius = value;
        }

        void Start()
        {
            Radius = startingRadius;
            Color = artboard.Painting.BackgroundColor;
        }
    }
}