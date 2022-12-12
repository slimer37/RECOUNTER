using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.InputSystem.Interactions;

public class OctoPlacer : MonoBehaviour
{
    [Header("Ghost")]
    [SerializeField] Ghost _ghost;
    [SerializeField] Material _freeMat;
    [SerializeField] Material _obstructedMat;

    [Header("Controls")]
    [SerializeField] InputActionReference startPlaceButton;
    [SerializeField] InputActionReference dropOrThrow;
    [SerializeField] InputActionReference holdRotateButton;
    [SerializeField] InputActionReference verticalAxis;
    [SerializeField] InputActionReference lateralDelta;

    [Header("Throwing")]
    [SerializeField] float _timeToFullCharge;
    [SerializeField] Vector3 _chargedPos;
    [SerializeField] float _throwForce;
    [SerializeField] Vector3 _throwDirection;

    [Header("Placing")]
    [SerializeField] float _lateralSpeed;
    [SerializeField] float _verticalSpeed;
    [SerializeField] float _surfaceSeparation;
    [SerializeField] LayerMask _obstacleMask;
    [SerializeField] LayerMask _lineOfSightMask;
    [SerializeField] float _smoothing;
    [SerializeField] Vector3 _placementRegionExtents;
    [SerializeField] Vector3 _placementRegionCenter;
    [SerializeField] float _startPlaceDistance;

    [Header("Rotation")]
    [SerializeField] float _rotateSpeed;
    [SerializeField] float _defaultRot = 180f;

    [Header("Holding")]
    [SerializeField] Vector3 _intersectHoldShift;
    [SerializeField] Vector3 _intersectHoldRotShift;
    [SerializeField] Vector3 _holdPos;
    [SerializeField] Vector3 _holdRot;
    [SerializeField] Transform _body;

    [Header("UI")]
    [SerializeField] Image _cursorImage;
    [SerializeField] Sprite _placeIcon;
    [SerializeField] Sprite _rotateIcon;
    [SerializeField] Sprite _defaultIcon;

    [Header("Layers")]
    [SerializeField, Layer] int _viewmodelLayer;
    [SerializeField, Layer] int _defaultLayer;

    [Header("Components")]
    [SerializeField] PlayerController _playerController;
    [SerializeField] PlayerInteraction _playerInteraction;
    [SerializeField] Camera _camera;

    Vector3 _localPlacePosition;
    float _localPlaceRotation;

    Vector3 _adjustedHoldPos;
    Quaternion _adjustedHoldRot;

    Item _active;
    bool _isPlacing;

    bool _isCharging;
    float _chargeTime = 0;

    bool _startPlaceObstructed;

    float _itemRotationVelocity;
    Vector3 _itemVelocity;

    public Item Active => _active;

    public bool IsPlacing => _isPlacing;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = _body.localToWorldMatrix;
        Gizmos.DrawWireCube(_placementRegionCenter, _placementRegionExtents * 2);
    }

    void Awake()
    {
        startPlaceButton.action.Enable();
        startPlaceButton.action.performed += OnStartPlace;

        dropOrThrow.action.Enable();
        dropOrThrow.action.performed += OnDropOrThrow;
        dropOrThrow.action.canceled += OnEndDropOrThrow;

        holdRotateButton.action.Enable();
        verticalAxis.action.Enable();
        lateralDelta.action.Enable();
    }

    public void SetItem(Item item, bool canResetPosition)
    {
        _active = item;

        _active.gameObject.SetActive(true);

        _adjustedHoldRot = _active.OverrideHoldRotation ?? Quaternion.Euler(_holdRot);
        _adjustedHoldPos = _holdPos + _active.HoldPosShift;

        _localPlaceRotation = _defaultRot;

        _ghost.CopyMesh(item);

        if (!canResetPosition) return;

        _active.transform.SetPositionAndRotation(_body.position, _body.rotation);
    }

    void OnStartPlace(InputAction.CallbackContext ctx)
    {
        if (!_active) return;

        if (_isPlacing)
            EndPlace();
        else
            InitializePlacement();
    }

    void OnDropOrThrow(InputAction.CallbackContext ctx)
    {
        if (!_active) return;

        if (ctx.interaction is TapInteraction)
            DropItem();
        else if (!_isPlacing)
            StartChargingThrow(0);
    }

    void StartChargingThrow(float initialTime)
    {
        _isCharging = true;
        _chargeTime = initialTime;
        _ghost.Hide();
    }

    void OnEndDropOrThrow(InputAction.CallbackContext ctx)
    {
        if (!_active || ctx.interaction is not HoldInteraction) return;

        if (_isPlacing)
            DropItem();
        else if (_isCharging)
            ThrowHeldItem();
    }

    void ThrowHeldItem()
    {
        _isCharging = false;

        if (_active.IsIntersecting(_obstacleMask))
            return;

        PreReleaseItem().Throw(_chargeTime * _throwForce * _camera.transform.TransformDirection(_throwDirection));
    }

    void HandleThrowCharge()
    {
        _chargeTime = Mathf.Clamp01(_chargeTime + Time.deltaTime / _timeToFullCharge);

        SetViewmodelLayer(true);

        var cameraLocalPos = Vector3.Slerp(_adjustedHoldPos, _chargedPos, _chargeTime);
        var cameraLocalRot = _camera.transform.rotation * _adjustedHoldRot;

        PullItemTo(_camera.transform.TransformPoint(cameraLocalPos), cameraLocalRot);
    }

    void Update()
    {
        if (!_active) return;

        if (_isCharging)
        {
            HandleThrowCharge();
            return;
        }

        if (!_isPlacing) return;

        var previousPos = _localPlacePosition;
        var previousRot = _localPlaceRotation;

        HandleVertical(verticalAxis.action.ReadValue<float>());

        var delta = lateralDelta.action.ReadValue<Vector2>();

        if (holdRotateButton.action.IsPressed())
            HandleRotation(delta);
        else
            HandleLateral(delta);

        RestrictPlacePosition(ref _localPlacePosition);

        var placeRot = GetWorldPlaceRot();

        if (ItemIntersectsAtPosition(_localPlacePosition, placeRot))
        {
            _localPlacePosition = previousPos;
            _localPlaceRotation = previousRot;

            placeRot = GetWorldPlaceRot();
        }

        var placePos = GetWorldPlacePos();

        PullItemTo(placePos, placeRot);

        _cursorImage.transform.position = _camera.WorldToScreenPoint(_active.transform.position);
    }

    void LateUpdate()
    {
        if (!_active || _isPlacing || _isCharging) return;

        KeepItemInHand();
        ShowPreviewGhost();
    }

    Vector3 GetWorldPlacePos() => _body.TransformPoint(_localPlacePosition);
    Quaternion GetWorldPlaceRot() => Quaternion.Euler(0, _body.transform.eulerAngles.y + _localPlaceRotation, 0);

    bool ItemIntersectsAtPosition(Vector3 localPosition, Quaternion rotation) =>
        _active.WouldIntersectAt(_body.TransformPoint(localPosition), rotation, _obstacleMask);

    bool IsLineOfSightBlocked(Vector3 localPosition)
    {
        var pos = _body.TransformPoint(localPosition);
        var camPos = _camera.transform.position;
        var dir = pos - _camera.transform.position;
        return Physics.Raycast(camPos, dir, Vector3.Distance(camPos, pos), _lineOfSightMask);
    }

    void HandleLateral(Vector2 delta)
    {
        _localPlacePosition += _lateralSpeed * new Vector3(delta.x, 0, delta.y);
        _cursorImage.sprite = _placeIcon;
        _cursorImage.transform.rotation = Quaternion.identity;
    }

    void HandleRotation(Vector2 delta)
    {
        _localPlaceRotation += delta.x * _rotateSpeed;
        _cursorImage.sprite = _rotateIcon;
        _cursorImage.transform.eulerAngles = -Vector3.forward * _localPlaceRotation;
    }

    void HandleVertical(float rawScroll)
    {
        if (rawScroll == 0) return;

        var scrollDir = rawScroll > 0 ? 1 : -1;

        var dir = scrollDir * Vector3.up;
        var moveDelta = _verticalSpeed * dir;

        if (ItemIntersectsAtPosition(_localPlacePosition + moveDelta, _active.transform.rotation)
            && Physics.Raycast(_localPlacePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
        {
            var length = hit.distance - _active.SizeAlong(dir) + _surfaceSeparation;
            moveDelta = length * dir;
        }

        _localPlacePosition += moveDelta;
    }

    void RestrictPlacePosition(ref Vector3 localPlacePos)
    {
        var restrictedPos = localPlacePos;
        var center = _placementRegionCenter;
        var extents = _placementRegionExtents;

        restrictedPos -= center;
        restrictedPos.x = Mathf.Clamp(restrictedPos.x, -extents.x, extents.x);
        restrictedPos.y = Mathf.Clamp(restrictedPos.y, -extents.y, extents.y);
        restrictedPos.z = Mathf.Clamp(restrictedPos.z, -extents.z, extents.z);
        restrictedPos += center;

        localPlacePos = restrictedPos;
    }

    Vector3 CalculateLocalStartPos()
    {
        var pitch = -_camera.transform.eulerAngles.x * Mathf.Deg2Rad;
        var localStartPos = Vector3.forward + Mathf.Tan(pitch) * Vector3.up;
        localStartPos *= _startPlaceDistance;
        localStartPos += Vector3.forward * _active.SizeAlong(Vector3.forward);
        localStartPos += _body.InverseTransformPoint(_camera.transform.position);

        return localStartPos;
    }

    void InitializePlacement()
    {
        if (_startPlaceObstructed)
            return;

        _isPlacing = true;

        _ghost.Hide();

        SetViewmodelLayer(false);
        _playerInteraction.enabled = false;
        _playerController.Suspend(true);
    }

    void EndPlace()
    {
        _playerInteraction.enabled = true;
        _playerController.Suspend(false);

        _isPlacing = false;

        _ghost.Hide();

        _cursorImage.sprite = _defaultIcon;

        _cursorImage.transform.SetPositionAndRotation(
            new Vector2(Screen.width, Screen.height) / 2, Quaternion.identity
            );
    }

    Item PreReleaseItem()
    {
        EndPlace();

        SetViewmodelLayer(false);

        var temp = _active;

        _active = null;

        return temp;
    }

    void DropItem()
    {
        _active.transform.SetPositionAndRotation(GetWorldPlacePos(), GetWorldPlaceRot());

        PreReleaseItem().Release();
    }

    void KeepItemInHand()
    {
        SetViewmodelLayer(true);

        _localPlacePosition = CalculateLocalStartPos();

        RestrictPlacePosition(ref _localPlacePosition);

        _startPlaceObstructed = IsLineOfSightBlocked(_localPlacePosition)
            || ItemIntersectsAtPosition(
            _localPlacePosition,
            Quaternion.Euler(Vector3.up * (_body.eulerAngles.y + _defaultRot))
            );

        var cameraLocalPos = _adjustedHoldPos;
        var cameraLocalRot = _camera.transform.rotation * _adjustedHoldRot;

        if (_startPlaceObstructed)
        {
            cameraLocalPos += _intersectHoldShift;
            cameraLocalRot *= Quaternion.Euler(_intersectHoldRotShift);
        }

        PullItemTo(_camera.transform.TransformPoint(cameraLocalPos), cameraLocalRot);
    }

    void ShowPreviewGhost()
    {
        var ghostRot = Quaternion.Euler(0, _body.transform.eulerAngles.y + _localPlaceRotation, 0);
        var ghostMat = _startPlaceObstructed ? _obstructedMat : _freeMat;
        _ghost.ShowAt(_body.TransformPoint(_localPlacePosition), ghostRot, ghostMat);
    }

    public void StopHoldingItem()
    {
        if (!_active) return;

        EndPlace();

        _active.gameObject.SetActive(false);

        _active = null;
    }

    void PullItemTo(Vector3 targetPos, Quaternion targetRot)
    {
        var active = _active.transform;

        var currRot = active.rotation;
        var delta = Quaternion.Angle(currRot, targetRot);
        if (delta > 0f)
        {
            var t = Mathf.SmoothDampAngle(delta, 0, ref _itemRotationVelocity, _smoothing);
            t = 1f - (t / delta);
            active.rotation = Quaternion.Slerp(currRot, targetRot, t);
        }

        active.position = Vector3.SmoothDamp(
            active.position,
            targetPos,
            ref _itemVelocity,
            _smoothing);
    }

    void SetViewmodelLayer(bool viewmodel)
    {
        var layer = viewmodel ? _viewmodelLayer : _defaultLayer;
        _active.gameObject.layer = layer;

        foreach (Transform child in _active.transform)
        {
            child.gameObject.layer = layer;
        }
    }
}
