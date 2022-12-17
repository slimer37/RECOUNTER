using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    [SerializeField] Transform _target;

    void LateUpdate()
    {
        transform.SetPositionAndRotation(_target.position, _target.rotation);
    }
}
