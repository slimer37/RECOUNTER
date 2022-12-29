using DG.Tweening;
using System;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] float _floorOffset;
    [SerializeField] float _raycastHeight;
    [SerializeField] float _plugDepth;
    [SerializeField] Transform _plug;

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

    Tween currentTween;

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

    public void SetStart(PowerInlet inlet, Vector3 wireAttach, Vector3 outward, Vector3 plugUp, Transform hookParent, Vector3 offset)
    {
        Inlet = inlet;

        OrientPlug(wireAttach, outward, plugUp);
        SetWireStart(wireAttach, outward);

        SetupHook(hookParent, offset);

        enabled = true;
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection, Vector3 plugUp)
    {
        currentTween?.Kill();

        Outlet = outlet;

        IsAnimating = true;

        var rotation = Quaternion.LookRotation(-plugDirection, plugUp);

        currentTween = DOTween.Sequence()
            .Append(_plug.DOMove(plugPoint + plugDirection * _plugOutOffset, _prePlugTime).SetEase(_prePlugEase))
            .Join(_plug.DORotateQuaternion(rotation, _prePlugTime).SetEase(_prePlugEase))
            .Append(_plug.DOMove(plugPoint, _plugTime).SetEase(_ease))
            .OnComplete(FinishConnect);
    }

    void FinishConnect()
    {
        Connected?.Invoke(Inlet, Outlet);

        IsAnimating = false;

        enabled = false;

        SetWireEnd();
        UpdateRenderer();
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

        currentTween = _plug
            .DOMove(_plug.position - _plug.forward * _plugOutOffset, _unplugTime)
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

    void SetWireStart(Vector3 attachPoint, Vector3 outward)
    {
        _wireStart = attachPoint + outward * _lineRenderer.startWidth / 2;

        var startPos = _wireStart;
        positions[0] = _wireStart;
        startPos.y = _floorOffset;
        positions[1] = startPos;
    }

    void OrientPlug(Vector3 position, Vector3 direction)
    {
        _plug.position = position;
        _plug.forward = -direction;
    }

    void OrientPlug(Vector3 position, Vector3 direction, Vector3 up)
    {
        OrientPlug(position, direction);
        _plug.rotation = Quaternion.LookRotation(-direction, up);
    }

    void SetWireEnd()
    {
        var wireEnd = _plug.position + -_plug.forward * _plugDepth;
        positions[^1] = wireEnd;
        wireEnd.y = _floorOffset;
        positions[^2] = wireEnd;
    }

    void FollowHook()
    {
        var hookPos = GetHookPosition();
        var direction = (_wireStart - hookPos).normalized;
        var smoothPos = Vector3.SmoothDamp(_plug.position, hookPos, ref _hookVelocity, _smoothTime);
        var smoothDirection = Vector3.Slerp(-_plug.forward, direction, Time.deltaTime / _smoothTime);
        OrientPlug(smoothPos, smoothDirection);
        SetWireEnd();
    }
}
