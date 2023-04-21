using UnityEngine;

namespace Recounter
{
    public enum PlacementCursor { None, Placement, Rotation }

    public class PlacementMethod : MonoBehaviour
    {
        [SerializeField] float _verticalSpeed;
        [SerializeField] float _lateralSpeed;
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] float _surfaceSeparation;
        [SerializeField] float _rotateSpeed;
        [SerializeField] float _startPlaceDistance;

        Placer _placer;
        Transform _body;
        Camera _camera;

        public Vector3 GetInitialPlacementPosition()
        {
            var pitch = -_camera.transform.eulerAngles.x * Mathf.Deg2Rad;
            var localStartPos = Vector3.forward + Mathf.Tan(pitch) * Vector3.up;
            localStartPos *= _startPlaceDistance;
            localStartPos += Vector3.forward * _placer.Active.SizeAlong(Vector3.forward);
            localStartPos += _body.InverseTransformPoint(_camera.transform.position);

            return _body.TransformPoint(localStartPos);
        }

        public bool IsItemPositionValid(Vector3 position, Quaternion rotation) =>
            _placer.Active.WouldIntersectAt(position, rotation, _obstacleMask);

        public void Initialize(Placer placer, Transform body, Camera camera)
        {
            _placer = placer;
            _body = body;
            _camera = camera;
        }

        public void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
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

        void HandleLateral(ref Vector3 placePosition, Vector2 delta)
        {
            placePosition += _lateralSpeed * _body.TransformDirection(delta.x, 0, delta.y);
        }

        void HandleRotation(ref Vector3 placeRotation, Vector2 delta)
        {
            placeRotation.y += delta.x * _rotateSpeed;
        }

        void HandleVertical(ref Vector3 placePosition, float rawScroll)
        {
            if (rawScroll == 0) return;

            var scrollDir = rawScroll > 0 ? 1 : -1;

            var dir = scrollDir * Vector3.up;
            var moveDelta = _verticalSpeed * dir;

            if (IsItemPositionValid(placePosition + moveDelta, _placer.Active.transform.rotation)
                && Physics.Raycast(placePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
            {
                var length = hit.distance - _placer.Active.SizeAlong(dir) + _surfaceSeparation;
                moveDelta = length * dir;
            }

            placePosition += moveDelta;
        }
    }
}
