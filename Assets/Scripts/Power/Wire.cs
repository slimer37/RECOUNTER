using DG.Tweening;
using FMODUnity;
using System;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] float _floorOffset;
    [SerializeField] float _raycastHeight;
    [SerializeField] float _plugDepth;
    [SerializeField] Transform _plug;
    [SerializeField] Vector3 _holdPosition;
    [SerializeField] Vector3 _holdRotation;

    [Header("SFX")]
    [SerializeField] float _plugInSfxDelay;
    [SerializeField] EventReference _plugSfx;

    [Header("Animation")]
    [SerializeField] float _prePlugTime;
    [SerializeField] Ease _prePlugEase;
    [SerializeField] float _plugTime;
    [SerializeField] float _plugOutOffset;
    [SerializeField] Ease _ease;
    [SerializeField] float _unplugTime;
    [SerializeField] Ease _unplugEase;

    Vector3 _wireStart;

    Vector3[] _positions;

    Tween _currentTween;

    Hand _hand;

    public Action<PowerInlet, PowerOutlet> Connected;
    public Action<PowerInlet, PowerOutlet> Disconnected;

    public bool IsAvailable => !_currentTween.IsActive() || !_currentTween.IsPlaying();

    public PowerInlet Inlet { get; private set; }
    public PowerOutlet Outlet { get; private set; }

    public Hand Holder => _hand;

    void Awake()
    {
        _positions = new Vector3[4];
        _lineRenderer.positionCount = _positions.Length;
    }

    public void SetStart(PowerInlet inlet, Vector3 wireAttach, Vector3 outward, Vector3 plugUp, Hand hand)
    {
        if (!hand)
            throw new ArgumentNullException(nameof(hand));

        if (hand.IsFull)
            throw new ArgumentException("The supplied hand is full.", nameof(hand));

        Inlet = inlet;

        OrientPlug(wireAttach, outward, plugUp);
        SetWireStart(wireAttach, outward);

        StartCarryingPlug(hand);
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection, Vector3 plugUp)
    {
        Invoke(nameof(PlaySfx), _plugInSfxDelay);

        _currentTween?.Kill();

        Outlet = outlet;

        var rotation = Quaternion.LookRotation(-plugDirection, plugUp);

        StopCarryingPlug();

        _currentTween = DOTween.Sequence()
            .Append(_plug.DOMove(plugPoint + plugDirection * _plugOutOffset, _prePlugTime).SetEase(_prePlugEase))
            .Join(_plug.DORotateQuaternion(rotation, _prePlugTime).SetEase(_prePlugEase))
            .Append(_plug.DOMove(plugPoint, _plugTime).SetEase(_ease))
            .OnComplete(FinishConnect);
    }

    void FinishConnect()
    {
        Connected?.Invoke(Inlet, Outlet);

        enabled = false;

        SetWireEnd();
        UpdateRenderer();
    }

    public void Disconnect(Hand hand)
    {
        if (!hand)
            throw new ArgumentNullException(nameof(hand));

        if (hand.IsFull)
            throw new ArgumentException("The supplied hand is full.", nameof(hand));

        if (!IsAvailable)
            throw new InvalidOperationException("Cannot disconnect while animating.");

        PlaySfx();

        Disconnected?.Invoke(Inlet, Outlet);

        StartCarryingPlug(hand);

        hand.SetReleaseState(HandReleaseState.FreePositionAndRotation | HandReleaseState.ResetLayer);

        _currentTween = _plug
            .DOMove(_plug.position - _plug.forward * _plugOutOffset, _unplugTime)
            .SetEase(_unplugEase)
            .OnComplete(FinishDisconnect);

        Outlet = null;
    }

    void FinishDisconnect()
    {
        _hand.SetReleaseState(HandReleaseState.None);
    }

    void LateUpdate()
    {
        SetWireEnd();
        UpdateRenderer();
    }

    void StartCarryingPlug(Hand hand)
    {
        _hand = hand;

        hand.Hold(_plug, _holdPosition, Quaternion.Euler(_holdRotation));

        enabled = true;
    }

    void StopCarryingPlug()
    {
        _hand.Clear();
        _hand = null;
    }

    void UpdateRenderer()
    {
        _lineRenderer.SetPositions(_positions);
    }

    void SetWireStart(Vector3 attachPoint, Vector3 outward)
    {
        _wireStart = attachPoint + outward * _lineRenderer.startWidth / 2;

        var startPos = _wireStart;
        _positions[0] = _wireStart;
        startPos.y = _floorOffset;
        _positions[1] = startPos;
    }

    void OrientPlug(Vector3 position, Vector3 direction, Vector3 up)
    {
        _plug.position = position;
        _plug.forward = -direction;
        _plug.rotation = Quaternion.LookRotation(-direction, up);
    }

    void SetWireEnd()
    {
        var wireEnd = _plug.position + -_plug.forward * _plugDepth;
        _positions[^1] = wireEnd;
        wireEnd.y = _floorOffset;
        _positions[^2] = wireEnd;
    }

    void PlaySfx()
    {
        if (_plugSfx.IsNull) return;

        RuntimeManager.PlayOneShotAttached(_plugSfx, _plug.gameObject);
    }
}
