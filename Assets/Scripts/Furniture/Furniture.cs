using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class Furniture : MonoBehaviour
    {
        [SerializeField] Material _highlight;

        public Vector3 Extents { get; private set; }
        public Vector3 CenterOffset { get; private set; }

        Renderer _renderer;

        List<Material> _materials = new();

        void OnDrawGizmosSelected()
        {
            var renderer = GetComponent<Renderer>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(renderer.localBounds.center, 0.025f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(renderer.localBounds.center, renderer.localBounds.size);
        }

        void Awake()
        {
            _renderer = GetComponent<Renderer>();

            Extents = Vector3.Scale(_renderer.localBounds.extents, transform.lossyScale);

            CenterOffset = _renderer.localBounds.center;

            _renderer.GetMaterials(_materials);
        }

        public void Highlight()
        {
            _materials.Add(_highlight);
            _renderer.SetMaterials(_materials);
        }

        public void RemoveHighlight()
        {
            _materials.Remove(_highlight);
            _renderer.SetMaterials(_materials);
        }

        Vector3 PositionFromGroundPoint(Vector3 groundPoint) => groundPoint + Vector3.up * (Extents.y + 0.01f) - CenterOffset;

        public void PlaceAt(Vector3 groundPoint, Quaternion rotation)
        {
            transform.SetPositionAndRotation(PositionFromGroundPoint(groundPoint), rotation);
        }

        public bool FitsAt(Vector3 groundPoint, Quaternion rotation, out Vector3 point)
        {
            point = PositionFromGroundPoint(groundPoint);

            return !Physics.CheckBox(point + CenterOffset, Extents, rotation);
        }
    }
}
