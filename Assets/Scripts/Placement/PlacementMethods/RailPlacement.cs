using NaughtyAttributes;
using UnityEngine;

namespace Recounter
{
    public class RailPlacement : MonoBehaviour, IPlacementMethod
    {
        [SerializeField, Tag] string _filterTag;
        [SerializeField] float _hangAmount;
        [SerializeField] float _speed;
        [SerializeField] float _modifierFactor;
        [SerializeField] float _extent;
        [SerializeField] Vector3 _rotationOffset;
        [SerializeField] float _spin;
        [SerializeField] Transform _initial;
        [SerializeField] float _maxDistanceFromCamera;

        Transform _body;
        Transform _camera;

        Plane _placementPlane;

        Vector3 _pivotPoint;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = _initial.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.left * _extent, Vector3.right * _extent);
        }

        void Awake()
        {
            _placementPlane = new Plane(_initial.forward, _initial.position);
        }

        public bool Accepts(Item item) => item.gameObject.CompareTag(_filterTag);

        public void SetUp(Placer placer, Transform body, Transform camera)
        {
            _body = body;
            _camera = camera;
        }

        Vector3 RailDirection() => _initial.right;

        Vector3 ConstrainToRail(Vector3 point)
        {
            var localX = Vector3.Dot(point - _initial.position, RailDirection());

            var cameraX = Vector3.Dot(_camera.transform.position - _initial.position, RailDirection());

            var leftBound = Mathf.Max(-_extent, cameraX - _maxDistanceFromCamera);
            var rightBound = Mathf.Min(_extent, cameraX + _maxDistanceFromCamera);

            localX = Mathf.Clamp(localX, leftBound, rightBound);

            return _initial.position + RailDirection() * localX;
        }

        Vector3 SpinRailPoint(Vector3 point) =>
            point + Quaternion.Euler(RailDirection() * _spin * Facing()) * Vector3.down * _hangAmount;

        float Facing() => Mathf.Sign(Vector3.Dot(_initial.forward, _camera.forward));

        public void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles)
        {
            var ray = new Ray(_camera.position, _camera.forward);

            _placementPlane.Raycast(ray, out var enter);

            _pivotPoint = ConstrainToRail(ray.GetPoint(enter));

            position = SpinRailPoint(_pivotPoint);

            eulerAngles = _initial.eulerAngles + (180f * (Facing() + 1) / 2) * Vector3.up + _rotationOffset;
        }

        public void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
        {
            var delta = _body.TransformDirection(new Vector3(mouseDelta.x, 0, mouseDelta.y));

            delta *= _speed * (modifier ? _modifierFactor : 1f);

            _pivotPoint += delta;

            _pivotPoint = ConstrainToRail(_pivotPoint);

            placePosition = SpinRailPoint(_pivotPoint);

            cursor = PlacementCursor.Placement;
        }
    }
}
