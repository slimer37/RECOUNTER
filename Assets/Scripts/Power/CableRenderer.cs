using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class CableRenderer : MonoBehaviour
{
    [SerializeField, Required] LineRenderer _lineRenderer;
    [SerializeField] int _resolution;
    [SerializeField, Min(0)] float _aPrecision = 0.0001f;
    [SerializeField, Min(0.0001f)] float _aIntervalStep = 0.01f;
    [SerializeField, Min(0.0001f)] float _length = 1f;
    [SerializeField] float _floorHeight;
    [SerializeField] int _maxIterations = 25;
    [SerializeField, Min(0)] float _breakStretch;
    [SerializeField] Color _breakingColor;

    [Header("Debug")]
    [SerializeField] bool _printIterations;

    Vector3[] _positions;

    Color _normalColor;

    void Awake()
    {
        var numPositions = 2 + _resolution;
        _lineRenderer.positionCount = numPositions;
        _positions = new Vector3[numPositions];
        _lineRenderer.SetPositions(_positions);
    }

    void SetLineColor(Color color)
    {
        _lineRenderer.material.color = color;
    }

    public void SetEndPositions(Vector3 start, Vector3 end)
    {
        if (start.y > end.y)
        {
            (start, end) = (end, start);
        }

        _positions[0] = start;
        _positions[^1] = end;

        var pointDistance = Vector3.Distance(start, end);
        var stretched = _length < pointDistance;

        if (stretched && _breakStretch > 0)
        {
            var stretchedAmount = pointDistance - _length;
            SetLineColor(Color.Lerp(_normalColor, _breakingColor, stretchedAmount / _breakStretch));
        }
        else
        {
            SetLineColor(_normalColor);
        }

        // X-coordinate is relative to the starting point.

        var startX = 0;
        var endX = new Vector3(end.x - start.x, 0, end.z - start.z).magnitude;

        var deltaX = endX;

        var deltaY = end.y - start.y;

        var a = CalculateA(deltaX, deltaY);

        for (int i = 1; i <= _resolution; i++)
        {
            // Set the position directly between the points.
            var t = (float)i / _positions.Length;
            var pos = Vector3.Lerp(start, end, t);

            if (!stretched)
            {
                var x = t * deltaX;
                pos.y = CalculateCatenary(startX + endX, start.y + end.y, deltaX, deltaY, x, a);
                pos.y = Mathf.Max(pos.y, _floorHeight);
            }

            _positions[i] = pos;
        }

        _lineRenderer.SetPositions(_positions);
    }

    static float Sinh(float x) => (float)Math.Sinh((double)x);
    static float Cosh(float x) => (float)Math.Cosh((double)x);
    static float Coth(float x) => Cosh(x) / Sinh(x);
    static float Ln(float x) => (float)Math.Log((double)x);

    float CalculateCatenary(float xSum, float ySum, float deltaX, float deltaY, float x, float a)
    {
        var p = xSum - a * Ln((_length + deltaY) / (_length - deltaY));
        p /= 2;
        var q = ySum - _length * Coth(deltaX / (2 * a));
        q /= 2;

        return a * Cosh((x - p) / a) + q;
    }

    float CalculateA(float deltaX, float deltaY)
    {
        var a = 0f;

        var a_prev = a;
        var a_next = a + _aIntervalStep;

        var compare = Mathf.Sqrt(Mathf.Pow(_length, 2) - Mathf.Pow(deltaY, 2)) / 2;

        var numIterations = 0;

        do
        {
            a = (a_prev + a_next) / 2f;

            if (a * Sinh(deltaX / (2 * a)) > compare)
                a_prev = a;
            else
                a_next = a;

            numIterations++;
        } while (numIterations < _maxIterations && a_next - a_prev > _aPrecision);

        if (_printIterations)
            print($"iterations: {numIterations} | result: {a}");

        return a;
    }
}
