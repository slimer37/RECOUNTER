using UnityEngine;

internal class Ghost : MonoBehaviour
{
    [SerializeField] MeshFilter filter;
    [SerializeField] MeshRenderer rend;
    [SerializeField] Material mat;

    void Reset()
    {
        rend = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();
    }

    void Start() => gameObject.SetActive(false);

    public void CopyMesh(Component source)
    {
        transform.localScale = source.transform.localScale;
        filter.mesh = source.GetComponentInChildren<MeshFilter>().mesh;

        var materials = new Material[filter.mesh.subMeshCount];
        for (var i = 0; i < materials.Length; i++)
            materials[i] = mat;

        rend.materials = materials;
    }

    public void ShowAt(Vector3 position, Quaternion rotation)
    {
        gameObject.SetActive(true);
        transform.position = position;
        transform.rotation = rotation;
    }

    public void Hide() => gameObject.SetActive(false);
}