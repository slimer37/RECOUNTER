using UnityEngine;

namespace Recounter
{
    public class AdjustableShelfBase : MonoBehaviour
    {
        [SerializeField] float _minHeight;
        [SerializeField] float _step;
        [SerializeField] float _forwardOffset;
        [SerializeField] int _maxShelves;

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

        public bool CheckAlignment(Vector3 position, out Vector3 aligned)
        {
            aligned = new Vector3();

            var localPosition = transform.InverseTransformPoint(position);
            localPosition.x = 0;

            var closestShelfIndex = Mathf.RoundToInt((localPosition.y - _minHeight) / _step);

            if (closestShelfIndex < 0 || closestShelfIndex >= _maxShelves) return false;

            localPosition.y = _minHeight + closestShelfIndex * _step;

            localPosition.z = _forwardOffset;

            aligned = transform.TransformPoint(localPosition);

            return true;
        }
    }
}
