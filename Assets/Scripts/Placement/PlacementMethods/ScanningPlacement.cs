using System;
using UnityEngine;

namespace Recounter
{
    public class ScanningPlacement : PlacementMethod
    {
        [SerializeField] Transform _initial;
        [SerializeField] float _offset;
        [SerializeField] float _sensitivity;
        [SerializeField] Vector3 _rotOffset;

        public event Action<Transform> ItemScanned;

        float _along;

        public override void CalculateInitialPosition()
        {
            PlacePosition = _initial.position - _initial.right * _offset + Vector3.up * ActiveItem.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);
            PlaceEulerAngles = _initial.eulerAngles + _rotOffset;
            _along = -_offset;
        }

        protected override void Move(bool modifier, Vector2 mouseDelta, float rawScroll)
        {
            var posToInitial = PlacePosition + _initial.right * mouseDelta.x * _sensitivity - _initial.position;

            var newAlong = Mathf.Clamp(Vector3.Dot(posToInitial, _initial.right), -_offset, _offset);

            if (_along < 0 && newAlong > 0 || _along > 0 && newAlong < 0)
            {
                ItemScanned.Invoke(ActiveItem.transform);
                print("Scan");
            }

            _along = newAlong;

            PlacePosition = _initial.position + _initial.right * _along;
            PlacePosition += Vector3.up * ActiveItem.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);
        }

        public override bool AttemptRelease() => false;
    }
}
