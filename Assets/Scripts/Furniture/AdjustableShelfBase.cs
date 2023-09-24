using System;
using NaughtyAttributes;
using UnityEngine;

namespace Recounter
{
    public class AdjustableShelfBase : MonoBehaviour
    {
        [SerializeField] float _minHeight;
        [SerializeField] float _step;
        [SerializeField] float _forwardOffset;
        [SerializeField] int _maxShelves;

        RemovableShelf[] _shelves;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;

            var basePoint = Vector3.up * _minHeight;
            var size = new Vector3(0.5f, 0.01f, 0.3f);

            for (int i = 0; i < _maxShelves; i++)
            {
                Gizmos.DrawCube(basePoint + _step * i * Vector3.up, size);
            }
        }

        [Button("Align")]
        void AlignShelves() => AlignShelves(false);

        void AlignShelves(bool assignIndices)
        {
            foreach (Transform shelf in transform)
            {
                if (!CheckAlignment(shelf.position, out var pos)) continue;

                shelf.transform.position = pos;

                if (!assignIndices) continue;

                var localPosition = transform.InverseTransformPoint(shelf.position);

                if (!MatchLocalPositionToShelfIndex(localPosition, out var index)) continue;

                _shelves[index] = shelf.GetComponent<RemovableShelf>();
                _shelves[index].InitializeToShelf(this);
            }
        }

        public void Detach(RemovableShelf shelfToRemove)
        {
            for (var i = 0; i < _shelves.Length; i++)
            {
                var shelf = _shelves[i];

                if (shelf != shelfToRemove) continue;
                
                _shelves[i] = null;
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(shelfToRemove), "Shelf was not found on this shelf base.");
        }

        void Awake()
        {
            _shelves = new RemovableShelf[_maxShelves];

            AlignShelves(true);
        }

        bool MatchLocalPositionToShelfIndex(Vector3 localPosition, out int index)
        {
            index = Mathf.RoundToInt((localPosition.y - _minHeight) / _step);
            return index > 0 && index < _maxShelves && _shelves[index] == null;
        }

        public bool CheckAlignment(Vector3 position, out Vector3 aligned)
        {
            aligned = new Vector3();

            var localPosition = transform.InverseTransformPoint(position);
            localPosition.x = 0;

            if (!MatchLocalPositionToShelfIndex(localPosition, out var closestShelfIndex))
                return false;

            localPosition.y = _minHeight + closestShelfIndex * _step;

            localPosition.z = _forwardOffset;

            aligned = transform.TransformPoint(localPosition);

            return true;
        }

        public bool TryAttach(RemovableShelf shelf)
        {
            var localPosition = transform.InverseTransformPoint(shelf.transform.position);

            if (!MatchLocalPositionToShelfIndex(localPosition, out var index)) return false;

            _shelves[index] = shelf;

            return true;
        }
    }
}
