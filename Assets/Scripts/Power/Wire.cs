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

    [Header("Hook")]
    [SerializeField] float _smoothTime;

    [Header("Animation")]
    [SerializeField] float _prePlugTime;
    [SerializeField] Ease _prePlugEase;
    [SerializeField] float _plugTime;
    [SerializeField] float _plugOutOffset;
    [SerializeField] Ease _ease;
    [SerializeField] float _unplugTime;
    [SerializeField] Ease _unplugEase;

    Vector3 _wireStart;
    Transform _hookParent;
    Vector3 _hookOffset;
    Vector3 _hookVelocity;

    Vector3[] positions;

    public Action<PowerInlet, PowerOutlet> Connected;
    public Action<PowerInlet, PowerOutlet> Disconnected;

    public PowerInlet Inlet { get; private set; }
    public PowerOutlet Outlet { get; private set; }
    public bool IsAnimating { get; private set; }

    void Awake()
    {
        positions = new Vector3[4];
        _lineRenderer.positionCount = positions.Length;
    }

    void OnEnable()
    {
        IsAnimating = false;
    }

    public void SetStart(PowerInlet inlet, Vector3 plugPoint, Vector3 plugDirection, Vector3 plugUp, Transform hookParent, Vector3 offset)
    {
        Inlet = inlet;

        OrientPlug(_plugStart, plugPoint, plugDirection, plugUp);
        OrientPlug(_plugEnd, plugPoint, plugDirection, plugUp);
        SetWireStart();

        SetupHook(hookParent, offset);

        enabled = true;
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection, Vector3 plugUp)
    {
        if (IsAnimating)
            throw new InvalidOperationException("Cannot connect while animating.");

        Outlet = outlet;

        IsAnimating = true;

        var rotation = Quaternion.LookRotation(-plugDirection, plugUp);

        DOTween.Sequence()
            .Append(_plugEnd.DOMove(plugPoint + plugDirection * _plugOutOffset, _prePlugTime).SetEase(_prePlugEase))
            .Join(_plugEnd.DORotateQuaternion(rotation, _prePlugTime).SetEase(_prePlugEase))
            .Append(_plugEnd.DOMove(plugPoint, _plugTime).SetEase(_ease))
            .OnComplete(FinishConnect);
    }

    void FinishConnect()
    {
        Connected?.Invoke(Inlet, Outlet);

        IsAnimating = false;

        enabled = false;
    }

    public void Disconnect(Transform hookParent, Vector3 offset)
    {
        if (IsAnimating)
            throw new InvalidOperationException("Cannot disconnect while animating.");

        WireManager.SetActiveWire(this);
        SetupHook(hookParent, offset);
        Disconnected?.Invoke(Inlet, Outlet);

        enabled = true;

        IsAnimating = true;

        _plugEnd
            .DOMove(_plugEnd.position - _plugEnd.forward * _plugOutOffset, _unplugTime)
            .SetEase(_unplugEase)
            .OnComplete(FinishDisconnect);

        Outlet = null;
    }

    void FinishDisconnect()
    {
        IsAnimating = false;
    }

    void Update()
    {
        if (IsAnimating)
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

    void OrientPlug(Transform plug, Vector3 position, Vector3 direction, Vector3 up)
    {
        OrientPlug(plug, position, direction);
        plug.rotation = Quaternion.LookRotation(-direction, up);
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
        var smoothPos = Vector3.SmoothDamp(_plugEnd.position, hookPos, ref _hookVelocity, _smoothTime);
        var smoothDirection = Vector3.Slerp(-_plugEnd.forward, direction, Time.deltaTime / _smoothTime);
        OrientPlug(_plugEnd, smoothPos, smoothDirection);
        SetWireEnd();
    }
}
