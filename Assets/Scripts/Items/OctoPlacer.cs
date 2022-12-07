using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class OctoPlacer : MonoBehaviour
{
    [SerializeField] Ghost _ghost;
    [SerializeField] Material _freeMat;
    [SerializeField] Material _obstructedMat;

    [Header("Placing")]
    [SerializeField] float _lateralSpeed;
    [SerializeField] float _verticalSpeed;
    [SerializeField] float _surfaceSeparation;
    [SerializeField] LayerMask _obstacleMask;
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

    public void SetItem(Item item, bool canResetPosition)
    {
        _active = item;

        _active.gameObject.SetActive(true);

        _adjustedHoldRot = _active.OverrideHoldRotation ?? Quaternion.Euler(_holdRot);
        _adjustedHoldPos = _holdPos + _active.HoldPosShift;

        _ghost.CopyMesh(item);

        if (!canResetPosition) return;

        _active.transform.SetPositionAndRotation(_body.position, _body.rotation);
    }

    void Update()
    {
        if (!_active) return;

        var mouse = Mouse.current;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            if (_isPlacing)
                _isPlacing = false;
            else
                InitializePlacement();
        }

        if (!_isPlacing)
        {
            EndPlace();
            KeepItemInHand();
            return;
        }

        StartPlace();

        var previousPos = _localPlacePosition;
        var previousRot = _localPlaceRotation;

        HandleVertical(mouse.scroll.ReadValue().y);

        var delta = mouse.delta.ReadValue();

        if (Keyboard.current.leftShiftKey.isPressed)
            HandleRotation(delta);
        else
            HandleLateral(delta);

        RestrictPlacePosition(ref _localPlacePosition);

        var placeRot = Quaternion.Euler(0, _body.transform.eulerAngles.y + _localPlaceRotation, 0);

        if (ItemIntersectsAtPosition(_localPlacePosition, placeRot))
        {
            _localPlacePosition = previousPos;
            _localPlaceRotation = previousRot;

            placeRot = Quaternion.Euler(0, _body.transform.eulerAngles.y + _localPlaceRotation, 0);
        }

        var placePos = _body.TransformPoint(_localPlacePosition);

        PullItemTo(placePos, placeRot);

        _cursorImage.transform.position = _camera.WorldToScreenPoint(_active.transform.position);

        if (mouse.leftButton.wasPressedThisFrame)
            DropItem();
    }

    bool ItemIntersectsAtPosition(Vector3 localPosition, Quaternion rotation) =>
        _active.WouldIntersectAt(_body.TransformPoint(localPosition), rotation, _obstacleMask);

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

        _localPlaceRotation = _defaultRot;
        _isPlacing = true;

        _ghost.Hide();
    }

    void StartPlace()
    {
        SetViewmodelLayer(false);
        _playerInteraction.enabled = false;
        _playerController.Suspend(true);
    }

    void EndPlace()
    {
        _playerInteraction.enabled = true;
        _playerController.Suspend(false);

        _isPlacing = false;

        _cursorImage.sprite = _defaultIcon;

        _cursorImage.transform.SetPositionAndRotation(
            new Vector2(Screen.width, Screen.height) / 2, Quaternion.identity
            );
    }

    void DropItem()
    {
        EndPlace();

        _active.transform.position = _body.TransformPoint(_localPlacePosition);

        var temp = _active;

        _active = null;

        temp.Release();
    }

    void KeepItemInHand()
    {
        SetViewmodelLayer(true);

        _localPlacePosition = CalculateLocalStartPos();

        RestrictPlacePosition(ref _localPlacePosition);

        var localRot = _camera.transform.rotation * _adjustedHoldRot;

        _startPlaceObstructed = ItemIntersectsAtPosition(
            _localPlacePosition,
            Quaternion.Euler(Vector3.up * (_body.eulerAngles.y + _defaultRot))
            );

        var cameraLocalPos = _adjustedHoldPos;

        if (_startPlaceObstructed)
        {
            cameraLocalPos += _intersectHoldShift;
            localRot *= Quaternion.Euler(_intersectHoldRotShift);
        }

        PullItemTo(_camera.transform.TransformPoint(cameraLocalPos), localRot);

        var ghostRot = Quaternion.Euler(0, _body.transform.eulerAngles.y + _localPlaceRotation, 0);
        var ghostMat = _startPlaceObstructed ? _obstructedMat : _freeMat;
        _ghost.ShowAt(_body.TransformPoint(_localPlacePosition), ghostRot, ghostMat);
    }

    public void StopHoldingItem()
    {
        if (!_active) return;

        EndPlace();

        _active.gameObject.SetActive(false);

        _ghost.Hide();

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
