using UnityEngine;

internal class Ghost : MonoBehaviour
{
    [SerializeField] MeshFilter _filter;
    [SerializeField] MeshRenderer _rend;
    [SerializeField] Material _mat;

    void Reset()
    {
        TryGetComponent(out _rend);
        TryGetComponent(out _filter);
    }

    void Start() => gameObject.SetActive(false);

    public void CopyMesh(Component source)
    {
        transform.localScale = source.transform.localScale;
        _filter.mesh = source.GetComponentInChildren<MeshFilter>().mesh;

        SetMaterial(_mat);
    }

    void SetMaterial(Material material)
    {
        var materials = new Material[_filter.mesh.subMeshCount];
        for (var i = 0; i < materials.Length; i++)
            materials[i] = material;

        _rend.materials = materials;
    }

    public void ShowAt(Vector3 position, Quaternion rotation, Material material)
    {
        ShowAt(position, rotation);
        SetMaterial(material);
    }

    public void ShowAt(Vector3 position, Quaternion rotation)
    {
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(position, rotation);
    }

    public void Hide() => gameObject.SetActive(false);
}