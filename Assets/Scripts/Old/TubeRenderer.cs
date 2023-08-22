using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TubeRenderer : MonoBehaviour
{
    [SerializeField, Min(1)] int _ringResolution = 4;
    [SerializeField, Min(0.0001f)] float _radius = 0.1f;
    [SerializeField] bool _worldSpace;
    [SerializeField] Vector3[] _positions = new Vector3[2];
    
    Vector3[] _vertices;

    int[] _triangles;

    Mesh _ropeMesh;

    MeshFilter _meshFilter;
    
    void Awake()
    {
        SetPositions(_positions);
    }

    void InitializeMeshIfNeeded()
    {
        if (_ropeMesh) return;

        _meshFilter = GetComponent<MeshFilter>();

        _ropeMesh = new Mesh
        {
            name = "Rope"
        };

        _meshFilter.mesh = _ropeMesh;
    }

    public void SetPositions(Vector3[] positions)
    {
        _positions = positions;

        _vertices = new Vector3[positions.Length * _ringResolution];
        _triangles = new int[(_vertices.Length - _ringResolution) * 6];

        InitializeMeshIfNeeded();
        GenerateMesh();
    }

    void GenerateMesh()
    {
        for (var i = 0; i < _positions.Length; i++)
        {
            var position = _positions[i];

            var vertIndex = i * _ringResolution;

            var forward = Vector3.zero;

            if (i > 0)
            {
                forward += position - _positions[i - 1];
            }

            if (i < _positions.Length - 1)
            {
                forward += _positions[i + 1] - position;
            }

            forward.Normalize();

            var center = position;

            if (_worldSpace)
            {
                center = transform.InverseTransformPoint(center);
            }

            for (int j = 0; j < _ringResolution; j++)
            {
                var theta = (float)j / _ringResolution * 2 * Mathf.PI;
                var x = _radius * Mathf.Cos(theta);
                var y = _radius * Mathf.Sin(theta);
                var pos = Vector3.up * y + Vector3.right * x;
                var localPos = Quaternion.FromToRotation(Vector3.forward, forward) * pos;
                _vertices[vertIndex + j] = center + localPos;
            }

            if (i == 0) continue;

            var triIndex = (i - 1) * _ringResolution * 6;

            for (int j = 0; j < _ringResolution - 1; j++)
            {
                _triangles[triIndex] = vertIndex;
                _triangles[triIndex + 1] = vertIndex - _ringResolution + 1;
                _triangles[triIndex + 2] = vertIndex + 1;

                _triangles[triIndex + 3] = vertIndex;
                _triangles[triIndex + 4] = vertIndex - _ringResolution;
                _triangles[triIndex + 5] = vertIndex - _ringResolution + 1;

                vertIndex++;
                triIndex += 6;
            }

            _triangles[triIndex] = vertIndex;
            _triangles[triIndex + 1] = vertIndex - 2 * _ringResolution + 1;
            _triangles[triIndex + 2] = vertIndex - _ringResolution + 1;

            _triangles[triIndex + 3] = vertIndex;
            _triangles[triIndex + 4] = vertIndex - _ringResolution;
            _triangles[triIndex + 5] = vertIndex - 2 * _ringResolution + 1;
        }

        _ropeMesh.Clear();
        _ropeMesh.SetVertices(_vertices);
        _ropeMesh.SetTriangles(_triangles, 0);
        _ropeMesh.RecalculateNormals();
    }
}
