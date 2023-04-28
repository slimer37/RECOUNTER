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

        IPlacementMethod _defaultMethod;

        Vector3 _worldPlacePosition;
        Vector3 _worldPlaceRotation;

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

        IPlacementMethod _placementMethod;

        public void SetPlacementMethod(IPlacementMethod placementMethod)
        {
            placementMethod.Initialize(this, _body, _camera.transform);
            _placementMethod = placementMethod;
        }

        public void ResetPlacementMethod()
        {
            if (_isPlacing) return;

            SetPlacementMethod(_defaultMethod);
        }

        void InitializeDefaultPlacementMethod()
        {
            if (!TryGetComponent(out _defaultMethod))
            {
                Debug.LogWarning("Could not find a default placement method attached to Placer.", this);
            }

            ResetPlacementMethod();
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

            InitializeDefaultPlacementMethod();
        }

        void OnPause(bool pause) => enabled = !pause;

        void OnDestroy() => Pause.Paused -= OnPause;

        public void SetItem(Item item, bool canResetPosition)
        {
            _active = item;

            _active.gameObject.SetActive(true);

            _adjustedHoldRot = _active.OverrideHoldRotation ?? Quaternion.Euler(_holdRot);
            _adjustedHoldPos = _holdPos + _active.HoldPosShift;

            _worldPlaceRotation = _body.eulerAngles + Vector3.up * _defaultRot;

            _ghost.CopyMesh(item);

            _hand.Hold(item, _adjustedHoldPos, _adjustedHoldRot);

            if (item.ViewmodelPose.IsValid)
            {
                _hand.SetHandViewmodel(item.ViewmodelPose);
            }

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

            AttemptDropItem();
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

            var previousPos = _worldPlacePosition;
            var previousRot = _worldPlaceRotation;

            var rawScroll = _verticalAxisAction.ReadValue<float>();
            var modifier = _holdRotateAction.IsPressed();
            var mouseDelta = _lateralMoveDelta.ReadValue<Vector2>();

            _placementMethod.HandlePlacement(ref _worldPlacePosition, ref _worldPlaceRotation, modifier, mouseDelta, rawScroll, out var placementCursor);

            if (!_placementMethod.IsItemPositionValid(_worldPlacePosition, Quaternion.Euler(_worldPlaceRotation)))
            {
                _worldPlacePosition = previousPos;
                _worldPlaceRotation = previousRot;
            }

            _hand.UpdateHoldPositionAndRotation(_worldPlacePosition, Quaternion.Euler(_worldPlaceRotation));

            ModifyCursor(placementCursor);
        }

        void LateUpdate()
        {
            if (!_active || _isPlacing || _isCharging) return;

            KeepItemInHand();
            ShowPreviewGhost();
        }

        bool IsLineOfSightBlocked(Vector3 worldPosition)
        {
            var camPos = _camera.transform.position;
            var dir = worldPosition - _camera.transform.position;
            return Physics.Raycast(camPos, dir, Vector3.Distance(camPos, worldPosition), _lineOfSightMask);
        }

        void ModifyCursor(PlacementCursor placementCursor)
        {
            if (placementCursor == PlacementCursor.None)
            {
                _cursorImage.overrideSprite = null;
                _cursorImage.transform.SetPositionAndRotation(
                    new Vector2(Screen.width, Screen.height) / 2, Quaternion.identity
                );
                return;
            }

            _cursorImage.transform.position = _camera.WorldToScreenPoint(_active.transform.position);

            if (placementCursor == PlacementCursor.Placement)
            {
                _cursorImage.overrideSprite = _placeIcon;
                _cursorImage.transform.rotation = Quaternion.identity;
            }
            else if (placementCursor == PlacementCursor.Rotation)
            {
                _cursorImage.overrideSprite = _rotateIcon;
                _cursorImage.transform.eulerAngles = -Vector3.forward * _worldPlaceRotation.magnitude;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException(nameof(placementCursor));
            }
        }

        void InitializePlacement()
        {
            if (_startPlaceObstructed) return;

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

            ModifyCursor(PlacementCursor.None);

            ResetPlacementMethod();
        }

        Item PreReleaseItem()
        {
            EndPlace();

            _hand.Clear();

            var temp = _active;

            _active = null;

            return temp;
        }

        void AttemptDropItem()
        {
            var rot = Quaternion.Euler(_worldPlaceRotation);

            if (!_placementMethod.IsItemPositionValid(_worldPlacePosition, rot))
            {
                EndPlace();
                return;
            }
            _active.transform.SetPositionAndRotation(_worldPlacePosition, rot);

            PreReleaseItem().Release();
        }

        void KeepItemInHand()
        {
            _worldPlacePosition = _placementMethod.GetInitialPlacementPosition();

            _worldPlaceRotation = _body.eulerAngles + Vector3.up * _defaultRot;

            _startPlaceObstructed = IsLineOfSightBlocked(_worldPlacePosition)
                || !_placementMethod.IsItemPositionValid(_worldPlacePosition, Quaternion.Euler(_worldPlaceRotation));

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
            var ghostRot = Quaternion.Euler(_worldPlaceRotation);
            var ghostMat = _startPlaceObstructed ? _obstructedMat : _freeMat;
            _ghost.ShowAt(_worldPlacePosition, ghostRot, ghostMat);
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
