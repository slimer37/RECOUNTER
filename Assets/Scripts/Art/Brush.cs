using System;
using UnityEngine;

namespace Recounter.Art
{
    public abstract class Brush : MonoBehaviour
    {
        [SerializeField] ComputeShader _computeShader;

        int _drawKernel;
        int _drawLineKernel;

        int _threadX;
        int _threadY;

        float _radius;

        public float Radius
        {
            get { return _radius; }

            protected set
            {
                _radius = value;
                _computeShader.SetFloat("Radius", value);
                RadiusChanged?.Invoke(value);
            }
        }

        public event Action<float> RadiusChanged;

        Color _color;

        public Color Color
        {
            get { return _color; }

            protected set
            {
                _color = value;
                _computeShader.SetFloats("Color", value.r, value.g, value.b, value.a);
                ColorChanged?.Invoke(value);
            }
        }

        public event Action<Color> ColorChanged;

        protected bool Active { get; private set; }

        protected virtual void Awake()
        {
            _drawKernel = _computeShader.FindKernel("Draw");
            _drawLineKernel = _computeShader.FindKernel("DrawLine");
            _computeShader = Instantiate(_computeShader);
        }

        public virtual void Activate(Texture texture)
        {
            Active = true;

            _threadX = texture.width / 8;
            _threadY = texture.height / 8;

            _computeShader.SetTexture(_drawKernel, "Result", texture);
            _computeShader.SetTexture(_drawLineKernel, "Result", texture);
        }

        public virtual void Deactivate()
        {
            Active = false;
        }

        void Dispatch(int kernelIndex) => _computeShader.Dispatch(kernelIndex, _threadX, _threadY, 1);

        public virtual void Draw(float x, float y)
        {
            _computeShader.SetFloats("A", x, y);
            Dispatch(_drawKernel);
        }

        public virtual void DrawContinuousLine(float x, float y)
        {
            _computeShader.SetFloats("B", x, y);
            Dispatch(_drawLineKernel);
            _computeShader.SetFloats("A", x, y);
        }
    }
}