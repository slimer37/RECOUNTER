using NaughtyAttributes;
using Recounter.Items;
using UnityEngine;

namespace Recounter
{
    public class RailPlacement : PlacementMethod
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

        public override bool Accepts(Placeable item) => item.gameObject.CompareTag(_filterTag);

        Vector3 RailDirection() => _initial.right;

        Vector3 ConstrainToRail(Vector3 point)
        {
            var localX = Vector3.Dot(point - _initial.position, RailDirection());

            var cameraX = Vector3.Dot(Camera.position - _initial.position, RailDirection());

            var leftBound = Mathf.Max(-_extent, cameraX - _maxDistanceFromCamera);
            var rightBound = Mathf.Min(_extent, cameraX + _maxDistanceFromCamera);

            localX = Mathf.Clamp(localX, leftBound, rightBound);

            return _initial.position + RailDirection() * localX;
        }

        Vector3 SpinRailPoint(Vector3 point) =>
            point + Quaternion.Euler(RailDirection() * _spin * Facing()) * Vector3.down * _hangAmount;

        float Facing() => Mathf.Sign(Vector3.Dot(_initial.forward, Camera.forward));

        public override void CalculateInitialPosition()
        {
            var ray = new Ray(Camera.position, Camera.forward);

            _placementPlane.Raycast(ray, out var enter);

            _pivotPoint = ConstrainToRail(ray.GetPoint(enter));

            PlacePosition = SpinRailPoint(_pivotPoint);

            PlaceEulerAngles = _initial.eulerAngles + (180f * (Facing() + 1) / 2) * Vector3.up + _rotationOffset;
        }

        protected override void Move(bool modifier, Vector2 mouseDelta, float rawScroll)
        {
            var delta = Body.TransformDirection(new Vector3(mouseDelta.x, 0, mouseDelta.y));

            delta *= _speed * (modifier ? _modifierFactor : 1f);

            _pivotPoint += delta;

            _pivotPoint = ConstrainToRail(_pivotPoint);

            PlacePosition = SpinRailPoint(_pivotPoint);
        }
    }
}
