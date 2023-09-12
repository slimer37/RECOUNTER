using System;
using UnityEngine;

namespace Recounter.Art
{
    public interface ITool
    {
        public void Draw(float x, float y);
        public void Activate(Texture texture);
        public void Deactivate();
    }

    public interface IBrush : ITool
    {
        public event Action<float> RadiusChanged;
        public event Action<Color> ColorChanged;
        
        public void DrawContinuousLine(float x, float y);
        
        public float Radius { get; }
        public Color Color { get; }
    }
}
