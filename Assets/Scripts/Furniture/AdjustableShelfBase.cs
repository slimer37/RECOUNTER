using UnityEngine;

namespace Recounter
{
    public class AdjustableShelfBase : MonoBehaviour
    {
        [SerializeField] float _minHeight;
        [SerializeField] float _maxHeight;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            var size = Vector3.up * (_maxHeight - _minHeight);
            var center = Vector3.up * _minHeight + size / 2;
            size += new Vector3(0.1f, 0f, 0.1f);
            Gizmos.DrawCube(center, size);
        }

        public Vector3 GetAlignment(Vector3 position)
        {
            var localPosition = transform.InverseTransformPoint(position);
            localPosition.x = 0;
            localPosition.y = Mathf.Clamp(localPosition.y, _minHeight, _maxHeight);
            return transform.TransformPoint(localPosition);
        }
    }
}
