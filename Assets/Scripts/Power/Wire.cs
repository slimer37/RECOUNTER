using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Obi;
using System;
using UnityEngine;

public class Wire : Interactable
{
    [SerializeField] ObiRope _rope;
    [SerializeField] float _raycastHeight;
    [SerializeField] Vector3 _holdPosition;
    [SerializeField] Vector3 _holdRotation;
    [SerializeField] Transform _startAttachment;
    [SerializeField] Transform _viewmodelPos;

    [Header("Plug")]
    [SerializeField] Transform _plug;
    [SerializeField] Rigidbody _plugRigidbody;
    [SerializeField] ObiParticleAttachment _plugAttachment;
    [SerializeField] ObiRigidbody _plugObiRb;
    [SerializeField] float _dropDistance;
    [SerializeField] float _dropDelay;

    [Header("SFX")]
    [SerializeField] float _plugInSfxDelay;
    [SerializeField] EventReference _plugSfx;

    [Header("Sparks")]
    [SerializeField] string _sparkSfxParam = "Spark";
    [SerializeField] ParticleSystem sparks;
    [SerializeField] float _sparkChance;

    [Header("Animation")]
    [SerializeField] float _prePlugTime;
    [SerializeField] Ease _prePlugEase;
    [SerializeField] float _plugTime;
    [SerializeField] float _plugOutOffset;
    [SerializeField] Ease _ease;
    [SerializeField] float _unplugDelay;
    [SerializeField] float _unplugTime;
    [SerializeField] Ease _unplugEase;

    Vector3 _wireStart;

    Tween _currentTween;

    Hand _hand;

    public Action<PowerInlet, PowerOutlet> Connected;
    public Action<PowerInlet, PowerOutlet> Disconnected;

    public bool IsAvailable => !_currentTween.IsActive() || !_currentTween.IsPlaying();

    public PowerInlet Inlet { get; private set; }
    public PowerOutlet Outlet { get; private set; }

    public Hand Holder => _hand;

    EventInstance _plugSfxInstance;
    EventInstance _unplugSfxInstance;

    bool shouldSpark;

    float _timeAtPickup;

    void OnEnable()
    {
        _plugSfxInstance = RuntimeManager.CreateInstance(_plugSfx);
        _unplugSfxInstance = RuntimeManager.CreateInstance(_plugSfx);

        _plugSfxInstance.set3DAttributes(_plug.position.To3DAttributes());
        _unplugSfxInstance.set3DAttributes(_plug.position.To3DAttributes());

        _rope.ResetParticles();
    }

    void OnDisable()
    {
        _plugSfxInstance.release();
        _unplugSfxInstance.release();
    }

    public void SetStart(PowerInlet inlet, Vector3 wireAttach, Hand hand)
    {
        if (!hand)
            throw new ArgumentNullException(nameof(hand));

        if (hand.IsFull)
            throw new ArgumentException("The supplied hand is full.", nameof(hand));

        Inlet = inlet;

        _plug.position = wireAttach;
        _plug.forward = Vector3.up;

        SetWireStart(wireAttach);

        StartCarryingPlug(hand);
    }

    public void Connect(PowerOutlet outlet, Vector3 plugPoint, Vector3 plugDirection, Vector3 plugUp)
    {
        _currentTween?.Kill();

        Outlet = outlet;

        var rotation = Quaternion.LookRotation(-plugDirection, plugUp);

        StopCarryingPlug();

        _currentTween = DOTween.Sequence()
            .Append(_plug.DOMove(plugPoint + plugDirection * _plugOutOffset, _prePlugTime).SetEase(_prePlugEase))
            .Join(_plug.DORotateQuaternion(rotation, _prePlugTime).SetEase(_prePlugEase))
            .Append(_plug.DOMove(plugPoint, _plugTime).SetEase(_ease))
            .OnComplete(FinishConnect);

        DecideSpark();
        Invoke(nameof(PlaySfx), _plugInSfxDelay);
    }

    void FinishConnect()
    {
        Connected?.Invoke(Inlet, Outlet);

        enabled = false;

        Spark();
    }

    protected override bool CanInteract(Employee e) => IsAvailable && !e.LeftHand.IsFull;

    protected override HudInfo FormHud(Employee e) => new()
    {
        icon = Outlet ? Icon.Unplug : Icon.Plug,
        text = Outlet ? "Unplug" : "Grab Plug"
    };

    protected override void OnInteract(Employee e)
    {
        if (Outlet)
        {
            Disconnect(e.LeftHand);
            return;
        }

        StartCarryingPlug(e.LeftHand);
    }

    void Disconnect(Hand hand)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Cannot disconnect while animating.");

        Disconnected?.Invoke(Inlet, Outlet);

        StartCarryingPlug(hand);

        hand.SetCarryStates(HandCarryStates.FreePositionAndRotation | HandCarryStates.ResetLayer);

        _currentTween = _plug
            .DOMove(_plug.position - _plug.forward * _plugOutOffset, _unplugTime)
            .SetDelay(_unplugDelay)
            .SetEase(_unplugEase)
            .OnComplete(FinishDisconnect);

        Outlet = null;

        DecideSpark();
        Spark();
        PlaySfx();
    }

    void FinishDisconnect()
    {
        _hand.SetCarryStates(HandCarryStates.None);
    }

    void StartCarryingPlug(Hand hand)
    {
        _timeAtPickup = Time.time;

        _hand = hand;

        hand.Hold(_plug, _holdPosition, Quaternion.Euler(_holdRotation));
        hand.SetHandViewmodel(_viewmodelPos);

        enabled = true;

        _plugRigidbody.isKinematic = true;
        _plugAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        _plugObiRb.enabled = false;
    }

    void LateUpdate()
    {
        if (!_hand || !IsAvailable) return;

        var timeSinceDisconnect = Time.time - _timeAtPickup;

        if (timeSinceDisconnect < _dropDelay) return;

        if (Vector3.Distance(_plug.position, _wireStart) > _dropDistance)
            DropPlug();
    }

    void DropPlug()
    {
        _currentTween?.Complete();

        StopCarryingPlug();

        _plugRigidbody.isKinematic = false;
        _plugAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        _plugObiRb.enabled = true;
    }

    void StopCarryingPlug()
    {
        _hand.Clear();
        _hand = null;
    }

    void SetWireStart(Vector3 attachPoint)
    {
        _rope.Teleport(attachPoint, Quaternion.identity);

        _rope.ResetParticles();

        _rope.TeleportParticle(_rope.elements[0].particle1, attachPoint);

        _wireStart = attachPoint;
        _startAttachment.position = _wireStart;
    }

    void DecideSpark()
    {
        shouldSpark = UnityEngine.Random.value <= _sparkChance;
    }

    void PlaySfx()
    {
        if (_plugSfx.IsNull) return;

        var isPluggingIn = (bool)Outlet;

        var instance = isPluggingIn ? _plugSfxInstance : _unplugSfxInstance;

        instance.set3DAttributes(_plug.position.To3DAttributes());

        instance.setParameterByName(_sparkSfxParam, shouldSpark ? 1 : 0);

        instance.start();
    }

    void Spark()
    {
        if (!shouldSpark) return;

        sparks?.Play();
    }
}
