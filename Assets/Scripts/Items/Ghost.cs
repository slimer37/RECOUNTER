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

    bool _canToggleVisibility;

    Material _currentMaterial;

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

        Hide();
    }

    void ToggleVisibility(InputAction.CallbackContext ctx)
    {
        if (!_canToggleVisibility) return;

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
        if (_currentMaterial == material) return;

        _currentMaterial = material;

        var materials = new Material[_filter.mesh.subMeshCount];
        for (var i = 0; i < materials.Length; i++)
            materials[i] = material;

        _rend.materials = materials;
    }

    public void ShowAt(Vector3 position, Quaternion rotation, Material material, bool forceVisible)
    {
        ShowGhost(forceVisible);

        transform.SetPositionAndRotation(position, rotation);

        SetMaterial(material);
    }

    public void Hide()
    {
        _canToggleVisibility = false;

        _instructionCanvas.enabled = false;

        _rend.enabled = false;
    }

    void ShowGhost(bool forceVisible = false)
    {
        _canToggleVisibility = !forceVisible;

        _instructionCanvas.enabled = _canToggleVisibility;

        _rend.enabled = _visibilityToggle || forceVisible;
    }
}