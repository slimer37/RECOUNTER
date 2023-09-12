using UnityEngine;

namespace Recounter.Art
{
    public class PresetBrush : Brush
    {
        [SerializeField] Color color;

        new public float Radius
        {
            get => base.Radius;
            set => base.Radius = value;
        }

        protected override void Awake()
        {
            base.Awake();

            Color = color;
        }
    }
}