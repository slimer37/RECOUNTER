using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter
{
    public class Placer : MonoBehaviour, IHoverHandler<IPlacementMethod>
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

        [Header("Masks")]
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] LayerMask _lineOfSightMask;

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

        [Header("Placement Methods")]
        [SerializeField] LayerMask _placementMethodMask;
        [SerializeField] LayerMask _placementMethodLayer;
        [SerializeField] float _placementMethodRange;

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
        IPlacementMethod _pendingMethod;

        HoverRaycaster<IPlacementMethod> _raycaster;

        void SetPlacementMethod(IPlacementMethod placementMethod)
        {
            if (_placementMethod == placementMethod) return;

            if (!placementMethod.Accepts(_active))
            {
                ResetPlacementMethod();
                return;
            }

            placementMethod.SetUp(this, _body, _camera.transform);
            _placementMethod = placementMethod;
        }

        void ResetPlacementMethod()
        {
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

            _raycaster = new(_camera, _placementMethodRange, _placementMethodMask, _placementMethodLayer, GetComponentType.Self)
            {
                TriggerInteraction = QueryTriggerInteraction.Collide
            };

            _raycaster.AssignCallbacks(this);
        }

        void OnPause(bool pause) => enabled = !pause;

        void OnDestroy() => Pause.Paused -= OnPause;

        public void SetItem(Item item, bool canResetPosition)
        {
            _active = item;

            _active.gameObject.SetActive(true);

            _adjustedHoldRot = _active.OverrideHoldRotation ?? Quaternion.Euler(_holdRot);
            _adjustedHoldPos = _holdPos + _active.HoldPosShift;

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

            if (!_isPlacing)
            {
                _raycaster.Raycast();
                return;
            }

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
            _cursorImage.overrideSprite = placementCursor switch
            {
                PlacementCursor.Placement => _placeIcon,
                PlacementCursor.Rotation => _rotateIcon,
                PlacementCursor.None => null,
                _ => throw new System.ArgumentOutOfRangeException(nameof(placementCursor))
            };

            var pos = new Vector2(Screen.width, Screen.height) / 2;
            var rot = Quaternion.identity;

            if (placementCursor != PlacementCursor.None) pos = _camera.WorldToScreenPoint(_active.transform.position);

            if (placementCursor == PlacementCursor.Rotation) rot = Quaternion.Euler(-Vector3.forward * _worldPlaceRotation.magnitude);

            _cursorImage.transform.SetPositionAndRotation(pos, rot);
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
            _placementMethod.GetInitialPositionAndRotation(out _worldPlacePosition, out _worldPlaceRotation);

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
            _ghost.ShowAt(_worldPlacePosition, ghostRot, ghostMat, _placementMethod.ShouldForceGhost);
        }

        public void StopHoldingItem()
        {
            if (!_active) return;

            EndPlace();

            _hand.Clear();

            _active.gameObject.SetActive(false);

            _active = null;
        }

        public void HoverEnter(IPlacementMethod obj)
        {
            _pendingMethod = obj;
            SetPlacementMethod(_pendingMethod);
        }

        public void HoverStay(IPlacementMethod obj)
        {
            if (_pendingMethod != null)
            {
                SetPlacementMethod(_pendingMethod);
            }
        }

        public void HoverExit(IPlacementMethod obj)
        {
            _pendingMethod = null;
            ResetPlacementMethod();
        }
    }
}
