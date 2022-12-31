using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] TubeRenderer _tubeRenderer;
    [SerializeField] int _nodeCount;
    [SerializeField] float _length;
    [SerializeField] Joint _templateNode;

    Rigidbody[] _rigidbodies;
    Vector3[] _positions;


    void Awake()
    {
        InitializeRigidbodies();

        _positions = new Vector3[_nodeCount];
    }

    void Update()
    {
        for (int i = 0; i < _nodeCount; i++)
        {
            _positions[i] = _rigidbodies[i].position;
        }

        _tubeRenderer.SetPositions(_positions);
    }

    void InitializeRigidbodies()
    {
        _rigidbodies = new Rigidbody[_nodeCount];

        var segmentLength = _length / _nodeCount;
        Joint previousJoint = null;

        for (int i = 0; i < _nodeCount; i++)
        {
            var joint = Instantiate(_templateNode, transform);
            var node = joint.transform;
            node.name = $"Rope Node ({i})";
            node.localPosition = Vector3.down * segmentLength * i;

            var rb = node.GetComponent<Rigidbody>();
            _rigidbodies[i] = rb;

            if (i == 0)
            {
                rb.isKinematic = true;
            }
            else
            {
                previousJoint.connectedBody = rb;
            }

            if (i == _nodeCount - 1)
            {
                Destroy(joint);
            }
            else
            {
                previousJoint = joint;
            }
        }

        _templateNode.gameObject.SetActive(false);
    }
}
