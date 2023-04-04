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

    [Header("Closing")]
    [SerializeField] Animator _viewmodelAnimator;
    [SerializeField, AnimatorParam(nameof(_viewmodelAnimator))] string _handClosedParam;
    [SerializeField, AnimatorParam(nameof(_viewmodelAnimator))] string _thumbCurlParam;

    [field: SerializeField, ReadOnly] public GameObject HeldObject { get; private set; }

    [ShowNonSerializedField] HandCarryStates _carryStates = HandCarryStates.None;

    public bool IsFull => HeldObject;

    public Vector3 HoldPosition { get; set; }
    public Quaternion HoldRot { get; set; }

    Vector3 _defaultHandPosition;
    Quaternion _defaultHandRotation;

    float _resetTime;
    float _handRotationVelocity;

    Transform _handTarget;

    Vector3 _positionVelocity;
    float _rotationVelocity;

    public HandCarryStates CarryStates => _carryStates;

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
    /// Sets the item-holding state according to <paramref name="states"/>
    /// but prevents other items from occupying this hand.
    /// </summary>
    /// <remarks>You do not need to unrelease the item before clearing the hand.</remarks>
    /// <param name="states">The new object release state.</param>
    public void SetCarryStates(HandCarryStates states)
    {
        if (!HeldObject)
            throw new InvalidOperationException("Cannot set release state with no held item.");

        var useViewmodelLayer = !states.HasFlag(HandCarryStates.ResetLayer);
        SetViewmodelLayer(useViewmodelLayer);

        _carryStates = states;
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
    public void SetHandViewmodel(ViewmodelPose pose)
    {
        _handTarget = pose.target;

        _viewmodelAnimator.SetFloat(_handClosedParam, pose.handClosedness);
        _viewmodelAnimator.SetFloat(_thumbCurlParam, pose.thumbCurl);
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

        _handTarget = null;

        _carryStates = HandCarryStates.None;

        _resetTime = 0;

        var temp = HeldObject;

        HeldObject = null;

        return temp;
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

        HandleViewmodel();
    }

    void HandleViewmodel()
    {
        var showViewmodel = !_carryStates.HasFlag(HandCarryStates.NoViewmodel) && HeldObject && _handTarget;

        if (showViewmodel)
        {
            _handViewmodelTarget.SetPositionAndRotation(
                HeldObject.transform.TransformPoint(_handTarget.localPosition),
                HeldObject.transform.rotation * _handTarget.localRotation);
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

        var localSpace = !_carryStates.HasFlag(HandCarryStates.WorldSpace);
        var controlPosition = !_carryStates.HasFlag(HandCarryStates.FreePosition);
        var controlRotation = !_carryStates.HasFlag(HandCarryStates.FreeRotation);

        if (localSpace)
        {
            targetPos = _followCamera.TransformPoint(targetPos);
            targetRot = _followCamera.rotation * targetRot;

            if (controlPosition)
            {
                var sin = _breathingIntensity * Mathf.Sin(Time.time * Mathf.PI * _breathingFrequency);
                targetPos += Vector3.up * sin;
            }
        }

        if (controlPosition)
        {
            SmoothDampPosition(
                HeldObject.transform,
                targetPos,
                ref _positionVelocity,
                _smoothing);
        }

        if (controlRotation)
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
            var t = Mathf.SmoothDamp(delta, 0, ref rotationVelocity, smoothing);

            t = 1f - (t / delta);
            transform.rotation = Quaternion.Slerp(currRot, targetRot, t);
        }
    }
}

[Serializable]
public struct ViewmodelPose
{
    public Transform target;
    public float handClosedness;
    public float thumbCurl;

    public bool IsValid => target;
}

[Flags]
public enum HandCarryStates
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
