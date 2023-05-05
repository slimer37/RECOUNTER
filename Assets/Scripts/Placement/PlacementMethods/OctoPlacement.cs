using UnityEngine;

namespace Recounter
{
    public class OctoPlacement : PlacementMethod
    {
        [SerializeField] float _verticalSpeed;
        [SerializeField] float _lateralSpeed;
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] float _surfaceSeparation;
        [SerializeField] float _startPlaceDistance;

        [Header("Rotation")]
        [SerializeField] float _defaultRot = 180f;
        [SerializeField] float _rotateSpeed;

        [Header("Cylindrical Bounds")]
        [SerializeField] float _radius;
        [SerializeField] float _yExtent;
        [SerializeField] float _forwardCutoff;
        [SerializeField] Vector3 _offset;

        public override bool ShouldForceGhost() => false;

        public override void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles)
        {
            var pitch = -Camera.eulerAngles.x * Mathf.Deg2Rad;
            var localStartPos = Vector3.forward + Mathf.Tan(pitch) * Vector3.up;
            localStartPos *= _startPlaceDistance;
            localStartPos += Vector3.forward * ActiveItem.SizeAlong(Vector3.forward);
            localStartPos += Body.InverseTransformPoint(Camera.position);

            position = Body.TransformPoint(localStartPos);

            eulerAngles = Body.eulerAngles + Vector3.up * _defaultRot;
        }

        public override bool IsItemPositionValid(Item item, Vector3 position, Quaternion rotation) =>
            !item.WouldIntersectAt(position, rotation, _obstacleMask);

        public override void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier,
            Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
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

            RestrictPlacePosition(ref placePosition);
        }

        void RestrictPlacePosition(ref Vector3 worldPlacePos)
        {
            var restrictedPos = Body.InverseTransformPoint(worldPlacePos);

            restrictedPos.z = Mathf.Max(restrictedPos.z, _forwardCutoff);

            restrictedPos -= _offset;

            // Restrict position based on cylindrical bounds (ignoring height)

            var temp = restrictedPos.y;
            restrictedPos.y = 0;

            restrictedPos = Vector3.ClampMagnitude(restrictedPos, _radius);

            restrictedPos.y = Mathf.Clamp(temp, -_yExtent, _yExtent);

            worldPlacePos = Body.TransformPoint(restrictedPos + _offset);
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

            if (IsItemPositionValid(ActiveItem, placePosition + moveDelta, ActiveItem.transform.rotation)
                && Physics.Raycast(placePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
            {
                var length = hit.distance - ActiveItem.SizeAlong(dir) + _surfaceSeparation;
                moveDelta = length * dir;
            }

            placePosition += moveDelta;
        }
    }
}
