using System;
using UnityEngine;

namespace Recounter
{
    public class ScanningPlacement : MonoBehaviour, IPlacementMethod
    {
        [SerializeField] Transform _initial;
        [SerializeField] float _offset;
        [SerializeField] float _sensitivity;
        [SerializeField] Vector3 _rotOffset;
        [SerializeField] LayerMask _obstacle;

        public event Action<Transform> ItemScanned;

        Placer _placer;

        float _along;

        public void SetUp(Placer placer, Transform body, Transform camera)
        {
            _placer = placer;
        }

        public void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles)
        {
            position = _initial.position - _initial.right * _offset + Vector3.up * _placer.Active.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);
            eulerAngles = _initial.eulerAngles + _rotOffset;
            _along = -_offset;
        }

        public void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
        {
            var posToInitial = placePosition + _initial.right * mouseDelta.x * _sensitivity - _initial.position;

            var newAlong = Mathf.Clamp(Vector3.Dot(posToInitial, _initial.right), -_offset, _offset);

            if (_along < 0 && newAlong > 0 || _along > 0 && newAlong < 0)
            {
                ItemScanned.Invoke(_placer.Active.transform);
                print("Scan");
            }

            _along = newAlong;

            placePosition = _initial.position + _initial.right * _along;
            placePosition += Vector3.up * _placer.Active.SizeAlong(Quaternion.Euler(_rotOffset) * Vector3.down);

            cursor = PlacementCursor.Placement;
        }

        public bool IsItemPositionValid(Vector3 position, Quaternion rotation) => !_placer.Active.WouldIntersectAt(position, rotation, _obstacle);
    }
}
