using UnityEngine;
using NaughtyAttributes;
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

    [Header("Viewmodel")]
    [SerializeField] Transform _handViewmodelTarget;
    [SerializeField] float _resetSpeed;

    [field: SerializeField, ReadOnly] public GameObject HeldObject { get; private set; }

    [ShowNonSerializedField] HandReleaseState _releaseState = HandReleaseState.None;

    public bool IsFull => HeldObject;

    public Vector3 HoldPosition { get; set; }
    public Quaternion HoldRot { get; set; }

    Vector3 _defaultHandPosition;
    Quaternion _defaultHandRotation;

    float _resetTime;
    float _handRotationVelocity;

    Transform _handTarget;
    Vector3 _handPositionOffset;
    Quaternion _handRotationOffset;

    Vector3 _positionVelocity;
    float _rotationVelocity;

    public HandReleaseState CurrentReleaseState => _releaseState;

    void Awake()
    {
        _handViewmodelTarget.parent = null;
        _defaultHandPosition = _followCamera.InverseTransformPoint(_handViewmodelTarget.position);
        _defaultHandRotation = Quaternion.Inverse(_followCamera.rotation) * _handViewmodelTarget.rotation;

        _resetTime = 1;
    }

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

        SetViewmodelLayer(true);
    }

    /// <summary>
    /// Sets the follow target for the hand viewmodel.
    /// </summary>
    public void SetHandViewmodel(Transform handTarget) =>
        SetHandViewmodel(handTarget, Vector3.zero, Quaternion.identity);

    /// <inheritdoc cref="SetHandViewmodel(Transform)"/>
    public void SetHandViewmodel(Transform handTarget, Vector3 positionOffset, Quaternion rotationOffset)
    {
        _handTarget = handTarget;
        _handPositionOffset = positionOffset;
        _handRotationOffset = rotationOffset;
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

        _handTarget = null;

        _releaseState = HandReleaseState.None;

        _resetTime = 0;

        return HeldObject;
    }

    void SetViewmodelLayer(bool viewmodel)
    {
        if (viewmodel == HeldObject.HierarchyLayersAreSet()) return;

        if (viewmodel)
            HeldObject.SetHierarchyLayers(_viewmodelLayer);
        else
            HeldObject.RestoreHierarchyLayers();
    }

    void LateUpdate()
    {
        if (HeldObject)
        {
            PullItemToHand();
        }

        var showViewmodel = !_releaseState.HasFlag(HandReleaseState.NoViewmodel) && HeldObject && _handTarget;

        if (showViewmodel)
        {
            _handViewmodelTarget.SetPositionAndRotation(
                _handTarget.TransformPoint(_handPositionOffset),
                _handTarget.rotation * _handRotationOffset);
        }
        else
        {
            if (_resetTime < 1)
                _resetTime += Time.deltaTime * _resetSpeed;

            _handViewmodelTarget.position =
                Vector3.Lerp(_handViewmodelTarget.position, _followCamera.TransformPoint(_defaultHandPosition), _resetTime);

            SmoothDampRotation(_handViewmodelTarget, _followCamera.rotation * _defaultHandRotation, ref _handRotationVelocity, _smoothing);
        }
    }

    public void ManualUpdate() => LateUpdate();

    void PullItemToHand()
    {
        var targetPos = HoldPosition;
        var targetRot = HoldRot;

        if (!_releaseState.HasFlag(HandReleaseState.WorldSpace))
        {
            targetPos = _followCamera.TransformPoint(targetPos);
            targetRot = _followCamera.transform.rotation * targetRot;
        }

        if (!_releaseState.HasFlag(HandReleaseState.FreePosition))
        {
            var sin = _breathingIntensity * Mathf.Sin(Time.time * Mathf.PI * _breathingFrequency);
            targetPos += Vector3.up * sin;

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
    NoViewmodel = 16,
    FreePositionAndRotation = FreePosition | FreeRotation | NoViewmodel,
    InWorld = ResetLayer | WorldSpace | NoViewmodel,
    All = FreePosition | FreeRotation | ResetLayer | NoViewmodel
}
