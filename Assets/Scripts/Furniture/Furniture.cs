using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class Furniture : MonoBehaviour
    {
        [SerializeField] Material _highlight;

        public Vector3 Extents { get; private set; }

        Renderer _renderer;

        List<Material> _materials = new();

        void Awake()
        {
            _renderer = GetComponent<Renderer>();

            Extents = Vector3.Scale(_renderer.localBounds.extents, transform.lossyScale);

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
    }
}
