using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Recounter
{
    public class Placer : MonoBehaviour
    {
        [Header("Hand")]
        [SerializeField] Hand _hand;

        [Header("Ghost")]
        [SerializeField] Ghost _ghost;
        [SerializeField] Material _freeMat;
        [SerializeField] Material _obstructedMat;

        [Header("Throwing")]
        [SerializeField] float _timeToFullCharge;
        [SerializeField] Vector3 _chargedPos;
        [SerializeField] float _throwForce;
        [SerializeField] Vector3 _throwDirection;

        [Header("Placing")]
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] LayerMask _lineOfSightMask;
        [SerializeField] Vector3 _placementRegionExtents;
        [SerializeField] Vector3 _placementRegionCenter;

        [Header("Rotation")]
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

        [Header("Components")]
        [SerializeField] PlayerInteraction _playerInteraction;
        [SerializeField] Camera _camera;
        [SerializeField] PlacementMethod _defaultMethod;

        Vector3 _localPlacePosition;
        Vector3 _localPlaceRotation;

        Vector3 _adjustedHoldPos;
        Quaternion _adjustedHoldRot;

        Item _active;
        bool _isPlacing;

        bool _isCharging;
        float _chargeTime = 0;

        bool _startPlaceObstructed;

        public Item Active => _active;

        public bool IsPlacing => _isPlacing;

        Controls.PlacementActions _placementControls;

        InputAction _holdRotateAction;
        InputAction _verticalAxisAction;
        InputAction _lateralMoveDelta;

        PlacementMethod _placementMethod;

        public void SetPlacementMethod(PlacementMethod placementMethod)
        {
            placementMethod.Initialize(this, _body, _camera);
            _placementMethod = placementMethod;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = _body.localToWorldMatrix;
            Gizmos.DrawWireCube(_placementRegionCenter, _placementRegionExtents * 2);
        }

        void Awake()
        {
            _placementControls = InputLayer.Placement;

            _placementControls.Place.performed += OnStartPlace;
            _placementControls.Place.canceled += OnEndPlace;

            _placementControls.Throw.performed += OnChargeThrow;
            _placementControls.Throw.canceled += OnThrow;

            _holdRotateAction = _placementControls.HoldRotate;
            _verticalAxisAction = _placementControls.VerticalMove;
            _lateralMoveDelta = _placementControls.Lateral;

            Pause.Paused += OnPause;

            SetPlacementMethod(_defaultMethod);
        }

        void OnPause(bool pause) => enabled = !pause;

        void OnDestroy() => Pause.Paused -= OnPause;

        public void SetItem(Item item, bool canResetPosition)
        {
            _active = item;

            _active.gameObject.SetActive(true);

            _adjustedHoldRot = _active.OverrideHoldRotation ?? Quaternion.Euler(_holdRot);
            _adjustedHoldPos = _holdPos + _active.HoldPosShift;

            _localPlaceRotation = Vector3.up * _defaultRot;

            _ghost.CopyMesh(item);

            _hand.Hold(item, _adjustedHoldPos, _adjustedHoldRot);

            if (item.ViewmodelPose.IsValid)
                _hand.SetHandViewmodel(item.ViewmodelPose);

            if (!canResetPosition) return;

            _active.transform.SetPositionAndRotation(_body.position, _body.rotation);
        }

        void OnStartPlace(InputAction.CallbackContext ctx)
        {
            if (!_active || _isPlacing || _isCharging) return;

            InitializePlacement();
        }

        void OnEndPlace(InputAction.CallbackContext ctx)
        {
            if (!_active || !_isPlacing) return;

            DropItem();
        }

        void OnChargeThrow(InputAction.CallbackContext ctx)
        {
            if (!_active) return;

            if (_isPlacing)
            {
                EndPlace();
                return;
            }

            _hand.SetCarryStates(HandCarryStates.NoViewmodel);

            StartChargingThrow();
        }

        void StartChargingThrow()
        {
            if (!_active.IsThrowable) return;

            _isCharging = true;
            _chargeTime = 0;
            _ghost.Hide();
        }

        void OnThrow(InputAction.CallbackContext ctx)
        {
            if (!_active || !_isCharging) return;

            _hand.SetCarryStates(HandCarryStates.None);

            ThrowHeldItem();
        }

        void ThrowHeldItem()
        {
            _isCharging = false;

            if (IsLineOfSightBlocked(_active.transform.position) || _active.IsIntersecting(_obstacleMask))
                return;

            PreReleaseItem().Throw(_chargeTime * _throwForce * _camera.transform.TransformDirection(_throwDirection));
        }

        void HandleThrowCharge()
        {
            _chargeTime = Mathf.Clamp01(_chargeTime + Time.deltaTime / _timeToFullCharge);

            var cameraLocalPos = Vector3.Slerp(_adjustedHoldPos, _chargedPos, _chargeTime);
            var cameraLocalRot = _adjustedHoldRot;

            _hand.UpdateHoldPositionAndRotation(cameraLocalPos, cameraLocalRot);
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

            _placementMethod.HandleVertical(ref _localPlacePosition, _verticalAxisAction.ReadValue<float>());

            var delta = _lateralMoveDelta.ReadValue<Vector2>();

            if (_holdRotateAction.IsPressed())
                HandleRotation(delta);
            else
                HandleLateral(delta);

            RestrictPlacePosition(ref _localPlacePosition);

            var placeRot = GetWorldPlaceRot();

            if (_placementMethod.IsItemPositionValid(_localPlacePosition, placeRot))
            {
                _localPlacePosition = previousPos;
                _localPlaceRotation = previousRot;

                placeRot = GetWorldPlaceRot();
            }

            var placePos = GetWorldPlacePos();

            _hand.UpdateHoldPositionAndRotation(placePos, placeRot);

            _cursorImage.transform.position = _camera.WorldToScreenPoint(_active.transform.position);
        }

        void LateUpdate()
        {
            if (!_active || _isPlacing || _isCharging) return;

            KeepItemInHand();
            ShowPreviewGhost();
        }

        Vector3 GetWorldPlacePos() => _body.TransformPoint(_localPlacePosition);
        Quaternion GetWorldPlaceRot() => Quaternion.Euler(_localPlaceRotation + _body.transform.eulerAngles);

        bool IsLineOfSightBlocked(Vector3 worldPosition)
        {
            var camPos = _camera.transform.position;
            var dir = worldPosition - _camera.transform.position;
            return Physics.Raycast(camPos, dir, Vector3.Distance(camPos, worldPosition), _lineOfSightMask);
        }

        void HandleLateral(Vector2 delta)
        {
            _placementMethod.HandleLateral(ref _localPlacePosition, delta);
            _cursorImage.overrideSprite = _placeIcon;
            _cursorImage.transform.rotation = Quaternion.identity;
        }

        void HandleRotation(Vector2 delta)
        {
            _placementMethod.HandleRotation(ref _localPlaceRotation, delta);
            _cursorImage.overrideSprite = _rotateIcon;
            _cursorImage.transform.eulerAngles = -Vector3.forward * _localPlaceRotation.magnitude;
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

        void InitializePlacement()
        {
            if (_startPlaceObstructed)
                return;

            _isPlacing = true;

            _ghost.Hide();

            _hand.SetCarryStates(HandCarryStates.InWorld);

            _playerInteraction.enabled = false;
            InputLayer.Movement.Disable();
            InputLayer.Movement.Crouch.Enable();
        }

        void EndPlace()
        {
            _playerInteraction.enabled = true;
            InputLayer.Movement.Enable();

            _isPlacing = false;

            _ghost.Hide();

            _hand.SetCarryStates(HandCarryStates.None);

            _cursorImage.overrideSprite = null;

            _cursorImage.transform.SetPositionAndRotation(
                new Vector2(Screen.width, Screen.height) / 2, Quaternion.identity
                );
        }

        Item PreReleaseItem()
        {
            EndPlace();

            _hand.Clear();

            var temp = _active;

            _active = null;

            return temp;
        }

        void DropItem()
        {
            if (_startPlaceObstructed) return;

            _active.transform.SetPositionAndRotation(GetWorldPlacePos(), GetWorldPlaceRot());

            PreReleaseItem().Release();
        }

        void KeepItemInHand()
        {
            _localPlacePosition = _placementMethod.CalculateLocalStartPos();

            RestrictPlacePosition(ref _localPlacePosition);

            var defaultPlaceRotation = Quaternion.Euler(Vector3.up * (_body.eulerAngles.y + _defaultRot));

            _startPlaceObstructed = IsLineOfSightBlocked(GetWorldPlacePos())
                || _placementMethod.IsItemPositionValid(_localPlacePosition, defaultPlaceRotation);

            var cameraLocalPos = _adjustedHoldPos;
            var cameraLocalRot = _adjustedHoldRot;

            if (_startPlaceObstructed)
            {
                cameraLocalPos += _intersectHoldShift;
                cameraLocalRot *= Quaternion.Euler(_intersectHoldRotShift);
            }

            _hand.UpdateHoldPositionAndRotation(cameraLocalPos, cameraLocalRot);
        }

        void ShowPreviewGhost()
        {
            var ghostRot = Quaternion.Euler(_body.transform.eulerAngles + _localPlaceRotation);
            var ghostMat = _startPlaceObstructed ? _obstructedMat : _freeMat;
            _ghost.ShowAt(_body.TransformPoint(_localPlacePosition), ghostRot, ghostMat);
        }

        public void StopHoldingItem()
        {
            if (!_active) return;

            EndPlace();

            _hand.Clear();

            _active.gameObject.SetActive(false);

            _active = null;
        }
    }
}
