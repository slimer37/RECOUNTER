using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class Furniture : MonoBehaviour
    {
        [SerializeField] Material _highlight;
        [SerializeField] Renderer _mainRenderer;

        public Vector3 Extents { get; private set; }
        public Vector3 CenterOffset { get; private set; }

        Renderer[] _renderers;

        bool _highlighted;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();

            Extents = Vector3.Scale(_mainRenderer.localBounds.extents, transform.lossyScale);

            CenterOffset = _mainRenderer.localBounds.center;

            if (!CheckFurnitureTag())
                Debug.LogWarning("This furniture is not tagged.", this);
        }

        bool CheckFurnitureTag()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.CompareTag("Furniture"))
                    return true;
            }

            return false;
        }

        public void Highlight()
        {
            if (_highlighted)
            {
                Debug.LogWarning("Furniture is already highlighted.", this);
                return;
            }

            List<Material> materials = new();

            foreach (var renderer in _renderers)
            {
                materials.Clear();
                renderer.GetSharedMaterials(materials);
                materials.Add(_highlight);
                renderer.SetSharedMaterials(materials);
            }

            _highlighted = true;
        }

        public void RemoveHighlight()
        {
            if (!_highlighted)
            {
                Debug.LogWarning("Furniture is not highlighted.", this);
                return;
            }

            List<Material> materials = new();

            foreach (var renderer in _renderers)
            {
                materials.Clear();
                renderer.GetSharedMaterials(materials);
                materials.RemoveAll(m => m == _highlight);
                renderer.SetSharedMaterials(materials);
            }

            _highlighted = false;
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
