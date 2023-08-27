using System;
using UnityEngine;

namespace Recounter
{
    public class FurniturePlacement : MonoBehaviour, IHoverHandler<Transform>
    {
        [SerializeField] LayerMask _placementMask;
        [SerializeField] LayerMask _allMask;
        [SerializeField] Ghost _ghost;
        [SerializeField] Material _ghostMat;
        [SerializeField] Material _badMat;
        [SerializeField] float _range;
        [SerializeField] float _rotateSensitivity;

        HoverRaycaster _raycaster;

        Furniture _furniture;

        Employee _employee;

        float _rotation;

        Vector3 _targetPoint;
        bool _validPoint;

        public bool IsActivated { get; private set; }

        Action _completedCallback;

        public void Activate(Employee e, Furniture furniture, Action completedCallback)
        {
            print("Activated!");

            IsActivated = true;
            _employee = e;
            e.Interaction.Suspend(true);
            _furniture = furniture;

            _ghost.CopyMesh(_furniture);

            _rotation = furniture.transform.eulerAngles.y;

            _completedCallback = completedCallback;

            _validPoint = false;
        }

        public void HoverEnter(Transform obj) { }

        public void HoverExit(Transform obj) => _ghost.Hide();

        public void HoverStay(Transform obj) { }

        public void OnRaycastHit(RaycastHit hit)
        {
            var rot = Quaternion.Euler(Vector3.up * _rotation);
            _validPoint = _furniture.FitsAt(hit.point, rot, out Vector3 point);

            if ((hit.point - transform.position).sqrMagnitude < _range * _range)
            {
                _targetPoint = point;
            }
            else
            {
                _validPoint = false;
            }

            _ghost.ShowAt(_targetPoint, rot, _validPoint ? _ghostMat : _badMat, true);
        }

        void Start()
        {
            _raycaster = new(Camera.main, Mathf.Infinity, _allMask, _placementMask, this);

            InputLayer.Placement.Lateral.performed += MoveLateral;
            InputLayer.Placement.HoldRotate.performed += HoldRotate_performed;
            InputLayer.Placement.HoldRotate.canceled += HoldRotate_canceled;
            InputLayer.Placement.Place.performed += Place;

            _ghost.Hide();
        }

        void Place(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsActivated) return;

            if (!_validPoint) return;

            _furniture.PlaceAt(_targetPoint, Quaternion.Euler(Vector3.up * _rotation));
            IsActivated = false;
            _employee.Interaction.Suspend(false);

            _ghost.Hide();
            InputLayer.Movement.Enable();

            _completedCallback();
        }

        void HoldRotate_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsActivated) return;
            InputLayer.Movement.Enable();
        }

        void HoldRotate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsActivated) return;
            InputLayer.Movement.Disable();
        }

        void MoveLateral(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsActivated) return;

            if (!InputLayer.Placement.HoldRotate.IsPressed()) return;

            _rotation += obj.ReadValue<Vector2>().x * _rotateSensitivity;
        }

        void Update()
        {
            if (!IsActivated) return;

            _raycaster.Raycast();
        }
    }
}
