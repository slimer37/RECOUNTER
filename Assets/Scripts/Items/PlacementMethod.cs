using UnityEngine;

namespace Recounter
{
    public class PlacementMethod : MonoBehaviour
    {
        [SerializeField] float _verticalSpeed;
        [SerializeField] float _lateralSpeed;
        [SerializeField] LayerMask _obstacleMask;
        [SerializeField] float _surfaceSeparation;
        [SerializeField] float _rotateSpeed;

        Placer _placer;
        Transform _body;

        public bool ItemIntersectsAtPosition(Vector3 localPosition, Quaternion rotation) =>
            _placer.Active.WouldIntersectAt(_body.TransformPoint(localPosition), rotation, _obstacleMask);

        public void Initialize(Placer placer, Transform body)
        {
            _placer = placer;
            _body = body;
        }

        public void HandleRotation(ref Vector3 localPlaceRotation, Vector2 delta)
        {
            localPlaceRotation.y += delta.x * _rotateSpeed;
        }

        public void HandleLateral(ref Vector3 localPlacePosition, Vector2 delta)
        {
            localPlacePosition += _lateralSpeed * new Vector3(delta.x, 0, delta.y);
        }

        public void HandleVertical(ref Vector3 localPlacePosition, float rawScroll)
        {
            if (rawScroll == 0) return;

            var scrollDir = rawScroll > 0 ? 1 : -1;

            var dir = scrollDir * Vector3.up;
            var moveDelta = _verticalSpeed * dir;

            if (ItemIntersectsAtPosition(localPlacePosition + moveDelta, _placer.Active.transform.rotation)
                && Physics.Raycast(localPlacePosition, dir, out var hit, _verticalSpeed, _obstacleMask))
            {
                var length = hit.distance - _placer.Active.SizeAlong(dir) + _surfaceSeparation;
                moveDelta = length * dir;
            }

            localPlacePosition += moveDelta;
        }
    }
}
