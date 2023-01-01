using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;

public class Hand : MonoBehaviour
{
    [SerializeField] float _smoothing;
    [SerializeField, Layer] int _viewmodelLayer;
    [SerializeField] Transform _followCamera;
    [SerializeField] Vector3 _defaultHoldPosition;
    [SerializeField] Vector3 _defaultHoldRotation;

    [Header("Breathing")]
    [SerializeField] float _breathingIntensity;
    [SerializeField] float _breathingFrequency;

    [field: SerializeField, ReadOnly] public GameObject HeldObject { get; private set; }

    [ShowNonSerializedField] HandReleaseState _releaseState = HandReleaseState.None;

    public bool IsFull => HeldObject;

    readonly List<int> _originalLayers = new();

    public Vector3 HoldPosition { get; set; }
    public Quaternion HoldRot { get; set; }

    Vector3 _positionVelocity;
    float _rotationVelocity;

    public HandReleaseState CurrentReleaseState => _releaseState;

    /// <summary>
    /// Checks for and retrieves (if it exists) a component on the item with the provided type.
    /// </summary>
    /// <typeparam name="T">The type of the behavior.</typeparam>
    /// <param name="result">The result; null if not found.</param>
    /// <returns>Whether the component was found.</returns>
    public bool Contains<T>(out T result) where T : class
    {
        if (!HeldObject)
        {
            result = null;
            return false;
        }

        return HeldObject.TryGetComponent(out result);
    }

    /// <summary>
    /// Checks for a component on the item's parent with the provided type.
    /// </summary>
    /// <inheritdoc cref="Contains{T}(out T)"/>
    public bool ContainsParentComponent<T>(out T result) where T : class
    {
        if (!HeldObject)
        {
            result = null;
            return false;
        }

        result = HeldObject.GetComponentInParent<T>();

        return result != null;
    }

    /// <summary>
    /// Releases the held item according to <paramref name="state"/>
    /// but prevents other items from occupying this hand.
    /// </summary>
    /// <remarks>You do not need to unrelease the item before clearing the hand.</remarks>
    /// <param name="state">The new object release state.</param>
    public void SetReleaseState(HandReleaseState state)
    {
        if (!HeldObject)
            throw new InvalidOperationException("Cannot set release state with no held item.");

        var useViewmodelLayer = !state.HasFlag(HandReleaseState.ResetLayer);
        SetViewmodelLayer(useViewmodelLayer);

        _releaseState = state;
    }

    /// <summary>
    /// Updates the holding position and rotation.
    /// </summary>
    /// <param name="holdPosition">The goal holding position, local to the camera.</param>
    /// <param name="holdRotation">The goal holding rotation, local to the camera.</param>
    public void UpdateHoldPositionAndRotation(Vector3 holdPosition, Quaternion holdRotation)
    {
        HoldPosition = holdPosition;
        HoldRot = holdRotation;
    }

    /// <summary>
    /// Sets the object this hand will hold by smoothly maintaining <paramref name="holdPosition"/> and <paramref name="holdRotation"/>.
    /// </summary>
    /// <param name="obj">The root GameObject to hold.</param>
    /// <param name="holdPosition">The goal holding position, local to the camera.</param>
    /// <param name="holdRotation">The goal holding rotation, local to the camera.</param>
    public void Hold(Component obj, Vector3 holdPosition, Quaternion holdRotation)
    {
        if (HeldObject)
            throw new InvalidOperationException("This hand is already holding an item.");

        UpdateHoldPositionAndRotation(holdPosition, holdRotation);

        HeldObject = obj.gameObject;

        _originalLayers.Clear();

        _originalLayers.Add(HeldObject.layer);

        foreach (var child in HeldObject.GetComponentsInChildren<Transform>())
        {
            _originalLayers.Add(child.gameObject.layer);
        }

        SetViewmodelLayer(true);
    }

    /// <inheritdoc cref="Hold(Component, Vector3, Quaternion)"/>
    public void Hold(Component obj) => Hold(obj, _defaultHoldPosition, Quaternion.Euler(_defaultHoldRotation));

    /// <inheritdoc cref="Hold(Component, Vector3, Quaternion)"/>
    public void Hold(Component obj, Vector3 holdPosition) => Hold(obj, holdPosition, Quaternion.Euler(_defaultHoldRotation));

    /// <inheritdoc cref="Hold(Component, Vector3, Quaternion)"/>
    public void Hold(Component obj, Quaternion holdRotation) => Hold(obj, _defaultHoldPosition, holdRotation);

    /// <summary>
    /// Removes the item held by this hand.
    /// </summary>
    /// <returns>The held object.</returns>
    public GameObject Clear()
    {
        if (!HeldObject)
            throw new InvalidOperationException("Cannot clear empty hand.");

        SetViewmodelLayer(false);

        HeldObject = null;

        _releaseState = HandReleaseState.None;

        return HeldObject;
    }

    void SetViewmodelLayer(bool viewmodel)
    {
        HeldObject.layer = viewmodel ? _viewmodelLayer : _originalLayers[0];

        var i = 1;
        foreach (var child in HeldObject.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = viewmodel ? _viewmodelLayer : _originalLayers[i++];
        }
    }

    void LateUpdate()
    {
        if (!HeldObject) return;

        PullItemToHand();
    }

    public void ManualUpdate() => LateUpdate();

    void PullItemToHand()
    {
        var targetPos = HoldPosition;
        var targetRot = HoldRot;

        if (_releaseState == HandReleaseState.None)
        {
            var sin = _breathingIntensity * Mathf.Sin(Time.time * Mathf.PI * _breathingFrequency);
            targetPos += Vector3.up * sin;
        }

        if (!_releaseState.HasFlag(HandReleaseState.WorldSpace))
        {
            targetPos = _followCamera.TransformPoint(targetPos);
            targetRot = _followCamera.transform.rotation * targetRot;
        }

        if (!_releaseState.HasFlag(HandReleaseState.FreePosition))
        {
            SmoothDampPosition(
                HeldObject.transform,
                targetPos,
                ref _positionVelocity,
                _smoothing);
        }

        if (!_releaseState.HasFlag(HandReleaseState.FreeRotation))
        {
            SmoothDampRotation(
                HeldObject.transform,
                targetRot,
                ref _rotationVelocity,
                _smoothing);
        }
    }

    static void SmoothDampPosition(Transform transform, Vector3 targetPos, ref Vector3 positionVelocity, float smoothing)
    {
        transform.position =
            Vector3.SmoothDamp(transform.position, targetPos, ref positionVelocity, smoothing);
    }

    static void SmoothDampRotation(Transform transform, Quaternion targetRot, ref float rotationVelocity, float smoothing)
    {
        var currRot = transform.rotation;
        var delta = Quaternion.Angle(currRot, targetRot);

        if (delta > 0f)
        {
            var t = Mathf.SmoothDampAngle(delta, 0, ref rotationVelocity, smoothing);

            t = 1f - (t / delta);
            transform.rotation = Quaternion.Slerp(currRot, targetRot, t);
        }
    }
}

[Flags]
public enum HandReleaseState
{
    None = 0,
    FreePosition = 1,
    FreeRotation = 2,
    ResetLayer = 4,
    WorldSpace = 8,
    FreePositionAndRotation = FreePosition | FreeRotation,
    InWorld = ResetLayer | WorldSpace,
    All = FreePosition | FreeRotation | ResetLayer
}
