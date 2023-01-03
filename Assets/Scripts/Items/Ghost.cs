using UnityEngine;
using UnityEngine.InputSystem;

internal class Ghost : MonoBehaviour
{
    [SerializeField] MeshFilter _filter;
    [SerializeField] MeshRenderer _rend;
    [SerializeField] Material _mat;

    [Header("Visibility")]
    [SerializeField] InputAction _toggleVisibilityAction;
    [SerializeField] Canvas _instructionCanvas;

    bool _visibilityToggle = true;

    bool _isActive;

    void Reset()
    {
        TryGetComponent(out _rend);
        TryGetComponent(out _filter);
    }

    void OnEnable() => _toggleVisibilityAction.Enable();
    void OnDisable() => _toggleVisibilityAction.Disable();

    void Awake()
    {
        _toggleVisibilityAction.performed += ToggleVisibility;

        SetupGhost(false);
    }

    void ToggleVisibility(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;

        _visibilityToggle = !_visibilityToggle;
        _rend.enabled = _visibilityToggle;
    }

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
        if (!_isActive)
            SetupGhost(true);

        transform.SetPositionAndRotation(position, rotation);
    }

    public void Hide() => SetupGhost(false);

    void SetupGhost(bool show)
    {
        _isActive = show;

        _rend.enabled = show && _visibilityToggle;
        _instructionCanvas.enabled = show;

        if (!show) return;
    }
}