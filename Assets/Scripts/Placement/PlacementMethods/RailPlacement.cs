using NaughtyAttributes;
using UnityEngine;

namespace Recounter
{
    public class RailPlacement : MonoBehaviour, IPlacementMethod
    {
        [SerializeField, Tag] string _filterTag;
        [SerializeField] Transform _initial;
        [SerializeField] float _speed;
        [SerializeField] float _modifierFactor;
        [SerializeField] float _extent;
        [SerializeField] float _yRotOffset;
        [SerializeField] float _maxDistanceFromCamera;
        [SerializeField] LayerMask _obstacleMask;

        Placer _placer;
        Transform _body;
        Transform _camera;

        Plane _placementPlane;

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
            _placer = placer;
            _body = body;
            _camera = camera;
        }

        Vector3 ConstrainToRail(Vector3 point)
        {
            var localPoint = _initial.InverseTransformPoint(point);

            var cameraPoint = _initial.InverseTransformPoint(_camera.transform.position);

            var leftBound = Mathf.Max(-_extent, cameraPoint.x - _maxDistanceFromCamera);
            var rightBound = Mathf.Min(_extent, cameraPoint.x + _maxDistanceFromCamera);

            localPoint.x = Mathf.Clamp(localPoint.x, leftBound, rightBound);

            localPoint.y = localPoint.z = 0;

            return _initial.TransformPoint(localPoint);
        }

        public void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles)
        {
            var ray = new Ray(_camera.position, _camera.forward);

            _placementPlane.Raycast(ray, out var enter);

            position = ConstrainToRail(ray.GetPoint(enter));

            var facing = (Mathf.Sign(Vector3.Dot(_initial.forward, _camera.forward)) + 1) / 2;

            eulerAngles = _initial.eulerAngles + (180f * facing + _yRotOffset) * Vector3.up;
        }

        public void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor)
        {
            var delta = _body.TransformDirection(new Vector3(mouseDelta.x, 0, mouseDelta.y));

            delta *= _speed * (modifier ? _modifierFactor : 1f);

            placePosition = ConstrainToRail(placePosition + delta);

            cursor = PlacementCursor.Placement;
        }

        public bool IsItemPositionValid(Vector3 position, Quaternion rotation)
        {
            Debug.DrawRay(position, rotation * Vector3.forward, Color.red, 0.01f);
            return !_placer.Active.WouldIntersectAt(position, rotation, _obstacleMask);
        }
    }
}
