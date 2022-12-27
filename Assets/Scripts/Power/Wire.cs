using System;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] float _floorOffset;
    [SerializeField] float _raycastHeight;
    [SerializeField] float _plugDepth;
    [SerializeField] Transform _plugStart;
    [SerializeField] Transform _plugEnd;

    Vector3 _wireStart;
    Transform _hookParent;
    Vector3 _hookOffset;

    Vector3[] positions;

    public Action Connected;

    public PowerInlet Inlet { get; private set; }
    public PowerOutlet Outlet { get; private set; }

    void Awake()
    {
        positions = new Vector3[4];
        _lineRenderer.positionCount = positions.Length;
    }

    public void SetStart(PowerInlet inlet, Vector3 plugPoint, Vector3 plugDirection, Transform hookParent, Vector3 offset)
    {
        Inlet = inlet;

        SetWireStart(plugPoint, plugDirection);

        SetupHook(hookParent, offset);

        enabled = true;
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection)
    {
        Outlet = outlet;

        SetWireEnd(plugPoint, plugDirection);

        UpdateRenderer();

        enabled = false;
        Connected?.Invoke();
    }

    void Update()
    {
        EvaluatePositions();

        UpdateRenderer();
    }

    void UpdateRenderer()
    {
        _lineRenderer.SetPositions(positions);
    }

    void SetupHook(Transform parent, Vector3 offset)
    {
        _hookParent = parent;
        _hookOffset = offset;
    }

    Vector3 GetHookPosition() => _hookParent.TransformPoint(_hookOffset);

    void SetWireStart(Vector3 plugPoint, Vector3 plugDirection)
    {
        _wireStart = plugPoint + plugDirection * _plugDepth;

        _plugStart.position = plugPoint;
        _plugStart.forward = -plugDirection;

        var startPos = _wireStart;
        positions[0] = _wireStart;

        startPos.y = _floorOffset;
        positions[1] = startPos;
    }

    void SetWireEnd(Vector3 plugPoint, Vector3 plugDirection)
    {
        _plugEnd.position = plugPoint;
        _plugEnd.forward = -plugDirection;

        var wireEnd = plugPoint + plugDirection * _plugDepth;
        positions[^1] = wireEnd;
        wireEnd.y = _floorOffset;
        positions[^2] = wireEnd;
    }

    void EvaluatePositions()
    {
        var hookPos = GetHookPosition();
        SetWireEnd(hookPos, (_wireStart - hookPos).normalized);
    }
}
