using Unity.VisualScripting;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] int _resolution;
    [SerializeField] float _length;
    [SerializeField] Joint _templateNode;

    Rigidbody[] _rigidbodies;
    Vector3[] _positions;

    void Awake()
    {
        _lineRenderer.positionCount = _resolution;
        _positions = new Vector3[_resolution];
        InitializeRigidbodies();
    }

    void InitializeRigidbodies()
    {
        _rigidbodies = new Rigidbody[_resolution];

        var segmentLength = _length / _resolution;
        Joint previousJoint = null;

        for (int i = 0; i < _resolution; i++)
        {
            var joint = Instantiate(_templateNode, transform);
            var node = joint.transform;
            node.name = $"Rope Node ({i})";
            node.localPosition = Vector3.down * segmentLength * i;

            var rb = node.GetComponent<Rigidbody>();
            _rigidbodies[i] = rb;

            if (i > 0)
            {
                previousJoint.connectedBody = rb;
            }

            if (i < _resolution - 1)
            {
                previousJoint = joint;
            }
        }

        _templateNode.gameObject.SetActive(false);
    }

    void Update()
    {
        for (var i = 0; i < _resolution; i++)
        {
            _positions[i] = _rigidbodies[i].position;
        }

        _lineRenderer.SetPositions(_positions);
    }
}
