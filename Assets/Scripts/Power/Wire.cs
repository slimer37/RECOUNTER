using DG.Tweening;
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

    [Header("Animation")]
    [SerializeField] float _prePlugTime;
    [SerializeField] Ease _prePlugEase;
    [SerializeField] float _plugTime;
    [SerializeField] float _plugStartOffset;
    [SerializeField] Ease _ease;

    Vector3 _wireStart;
    Transform _hookParent;
    Vector3 _hookOffset;

    Vector3[] positions;

    public Action<PowerInlet, PowerOutlet> Connected;
    public Action<PowerInlet, PowerOutlet> Disconnected;

    public PowerInlet Inlet { get; private set; }
    public PowerOutlet Outlet { get; private set; }
    public bool IsConnecting { get; private set; }

    void Awake()
    {
        positions = new Vector3[4];
        _lineRenderer.positionCount = positions.Length;
    }

    void OnEnable()
    {
        IsConnecting = false;
    }

    public void SetStart(PowerInlet inlet, Vector3 plugPoint, Vector3 plugDirection, Transform hookParent, Vector3 offset)
    {
        Inlet = inlet;

        OrientPlug(_plugStart, plugPoint, plugDirection);
        SetWireStart();

        SetupHook(hookParent, offset);

        enabled = true;
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection)
    {
        Outlet = outlet;

        IsConnecting = true;

        DOTween.Sequence()
            .Append(_plugEnd.DOMove(plugPoint + plugDirection * _plugStartOffset, _prePlugTime).SetEase(_prePlugEase))
            .Join(_plugEnd.DOLookAt(_plugEnd.position - plugDirection, _prePlugTime).SetEase(_prePlugEase))
            .Append(_plugEnd.DOMove(plugPoint, _plugTime).SetEase(_ease))
            .OnComplete(FinishConnect);
    }

    void FinishConnect()
    {
        Connected?.Invoke(Inlet, Outlet);

        IsConnecting = false;

        enabled = false;
    }

    public void Disconnect(Transform hookParent, Vector3 offset)
    {
        if (IsConnecting)
            throw new InvalidOperationException("Cannot disconnect during connect animation.");

        WireManager.SetActiveWire(this);
        SetupHook(hookParent, offset);
        enabled = true;

        Disconnected?.Invoke(Inlet, Outlet);

        Outlet = null;
    }

    void Update()
    {
        if (IsConnecting)
            SetWireEnd();
        else
            FollowHook();

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

    void SetWireStart()
    {
        _wireStart = _plugStart.position + -_plugStart.forward * _plugDepth;

        var startPos = _wireStart;
        positions[0] = _wireStart;
        startPos.y = _floorOffset;
        positions[1] = startPos;
    }

    void OrientPlug(Transform plug, Vector3 position, Vector3 direction)
    {
        plug.position = position;
        plug.forward = -direction;
    }

    void SetWireEnd()
    {
        var wireEnd = _plugEnd.position + -_plugEnd.forward * _plugDepth;
        positions[^1] = wireEnd;
        wireEnd.y = _floorOffset;
        positions[^2] = wireEnd;
    }

    void FollowHook()
    {
        var hookPos = GetHookPosition();
        var direction = (_wireStart - hookPos).normalized;
        OrientPlug(_plugEnd, hookPos, direction);
        SetWireEnd();
    }
}
