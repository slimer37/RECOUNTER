using UnityEngine;

namespace Recounter
{
    public class OctoPlacement : PlacementMethod
    {
        [SerializeField] float _verticalSpeed;
        [SerializeField] float _lateralSpeed;
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] float _surfaceSeparation;
        [SerializeField] float _rotateSpeed;
        [SerializeField] float _startPlaceDistance;

        public override Vector3 GetInitialPlacementPosition()
        {
            var pitch = -Camera.transform.eulerAngles.x * Mathf.Deg2Rad;
            var localStartPos = Vector3.forward + Mathf.Tan(pitch) * Vector3.up;
            localStartPos *= _startPlaceDistance;
            localStartPos += Vector3.forward * Placer.Active.SizeAlong(Vector3.forward);
            localStartPos += Body.InverseTransformPoint(Camera.transform.position);

            return Body.TransformPoint(localStartPos);
        }

        public override bool IsItemPositionValid(Vector3 position, Quaternion rotation) =>
            Placer.Active.WouldIntersectAt(position, rotation, _obstacleMask);

        public override void HandlePlacement(
            ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
        {
            HandleVertical(ref placePosition, rawScroll);

            cursor = PlacementCursor.Placement;

            if (modifier)
            {
                HandleRotation(ref placeRotation, mouseDelta);

                cursor = PlacementCursor.Rotation;
            }
            else
            {
                HandleLateral(ref placePosition, mouseDelta);
            }
        }

        void HandleLateral(ref Vector3 placePosition, Vector2 delta) =>
            placePosition += _lateralSpeed * Body.TransformDirection(delta.x, 0, delta.y);

        void HandleRotation(ref Vector3 placeRotation, Vector2 delta) =>
            placeRotation.y += delta.x * _rotateSpeed;

        void HandleVertical(ref Vector3 placePosition, float rawScroll)
        {
            if (rawScroll == 0) return;

            var scrollDir = rawScroll > 0 ? 1 : -1;

            var dir = scrollDir * Vector3.up;
            var moveDelta = _verticalSpeed * dir;

            if (IsItemPositionValid(placePosition + moveDelta, Placer.Active.transform.rotation)
                && Physics.Raycast(placePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
            {
                var length = hit.distance - Placer.Active.SizeAlong(dir) + _surfaceSeparation;
                moveDelta = length * dir;
            }

            placePosition += moveDelta;
        }
    }
}
