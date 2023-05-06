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

        public override void CalculateInitialPosition()
        {
            var pitch = -Camera.eulerAngles.x * Mathf.Deg2Rad;
            var localStartPos = Vector3.forward + Mathf.Tan(pitch) * Vector3.up;
            localStartPos *= _startPlaceDistance;
            localStartPos += Vector3.forward * ActiveItem.SizeAlong(Vector3.forward);
            localStartPos += Body.InverseTransformPoint(Camera.position);

            PlacePosition = Body.TransformPoint(localStartPos);

            PlaceEulerAngles = Body.eulerAngles + Vector3.up * _defaultRot;
        }

        public override bool IsPositionValid() => !ActiveItem.WouldIntersectAt(PlacePosition, PlaceRotation, _obstacleMask);

        protected override void Move(bool modifier, Vector2 mouseDelta, float rawScroll)
        {
            HandleVertical(rawScroll);

            Cursor = PlacementCursor.Placement;

            if (modifier)
            {
                HandleRotation(mouseDelta);

                Cursor = PlacementCursor.Rotation;
            }
            else
            {
                HandleLateral(mouseDelta);
            }

            RestrictPlacePosition();
        }

        void RestrictPlacePosition()
        {
            var restrictedPos = Body.InverseTransformPoint(PlacePosition);

            restrictedPos.z = Mathf.Max(restrictedPos.z, _forwardCutoff);

            restrictedPos -= _offset;

            // Restrict position based on cylindrical bounds (ignoring height)

            var temp = restrictedPos.y;
            restrictedPos.y = 0;

            restrictedPos = Vector3.ClampMagnitude(restrictedPos, _radius);

            restrictedPos.y = Mathf.Clamp(temp, -_yExtent, _yExtent);

            PlacePosition = Body.TransformPoint(restrictedPos + _offset);
        }

        void HandleLateral(Vector2 delta) =>
            PlacePosition += _lateralSpeed * Body.TransformDirection(delta.x, 0, delta.y);

        void HandleRotation(Vector2 delta) =>
            PlaceEulerAngles += Vector3.up * delta.x * _rotateSpeed;

        void HandleVertical(float rawScroll)
        {
            if (rawScroll == 0) return;

            var scrollDir = rawScroll > 0 ? 1 : -1;

            var dir = scrollDir * Vector3.up;
            var moveDelta = _verticalSpeed * dir;

            if (!ActiveItem.WouldIntersectAt(PlacePosition + moveDelta, ActiveItem.transform.rotation, _obstacleMask)
                && Physics.Raycast(PlacePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
            {
                var length = hit.distance - ActiveItem.SizeAlong(dir) + _surfaceSeparation;
                moveDelta = length * dir;
            }

            PlacePosition += moveDelta;
        }
    }
}
