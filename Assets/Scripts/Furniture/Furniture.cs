using Recounter.Store;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class Furniture : Interactable
    {
        [SerializeField] Material _highlight;
        [SerializeField] Renderer _mainRenderer;
        [SerializeField] SavableTransform _savableTransform;

        public Vector3 Extents { get; private set; }
        public Vector3 CenterOffset { get; private set; }

        Renderer[] _renderers;

        bool _highlighted;
        bool _isPlaced = true;

        HandTruck _mover;

        protected override bool CanInteract(Employee e) => !_isPlaced;

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = Icon.Hand,
            text = "Move furniture"
        };

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

        public void LoadToMover(HandTruck mover)
        {
            _mover = mover;
            _isPlaced = false;
        }

        protected override void OnInteract(Employee e) => _mover.BeginFurniturePlacement(e);

        public void PlaceAt(Vector3 groundPoint, Quaternion rotation)
        {
            SetColliders(true);
            
            transform.SetPositionAndRotation(PositionFromGroundPoint(groundPoint), rotation);
            _savableTransform.Save(true, true);
            _isPlaced = true;
        }

        public void SetColliders(bool enabled)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = enabled;
            }
        }

        public bool FitsAt(Vector3 groundPoint, Quaternion rotation, out Vector3 point)
        {
            point = PositionFromGroundPoint(groundPoint);

            return !Physics.CheckBox(point + CenterOffset, Extents, rotation);
        }
    }
}
