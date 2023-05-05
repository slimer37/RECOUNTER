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

        public override void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles)
        {
            position = _initial.position - _initial.right * _offset + Vector3.up * ActiveItem.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);
            eulerAngles = _initial.eulerAngles + _rotOffset;
            _along = -_offset;
        }

        public override void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier,
            Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
        {
            var posToInitial = placePosition + _initial.right * mouseDelta.x * _sensitivity - _initial.position;

            var newAlong = Mathf.Clamp(Vector3.Dot(posToInitial, _initial.right), -_offset, _offset);

            if (_along < 0 && newAlong > 0 || _along > 0 && newAlong < 0)
            {
                ItemScanned.Invoke(ActiveItem.transform);
                print("Scan");
            }

            _along = newAlong;

            placePosition = _initial.position + _initial.right * _along;
            placePosition += Vector3.up * ActiveItem.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);

            cursor = PlacementCursor.Placement;
        }

        public override bool AttemptRelease(Item item, Vector3 position, Quaternion rotation) => false;
    }
}
