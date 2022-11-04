using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] Camera cam;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] LayerMask interactableLayer;

    static readonly Vector3 ViewportCenter = new(0.5f, 0.5f);

    Interactable hovered;

    Controls.PlayerActions playerControls;

    void OnDrawGizmosSelected()
    {
        if (!cam) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cam.transform.position, range);
    }

    void Awake()
    {
        playerControls = new Controls().Player;
        playerControls.Interact.performed += OnInteract;
        text.text = "";
    }

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();

    void OnInteract(InputAction.CallbackContext context) => hovered?.Interact();

    void Update()
    {
        var ray = cam.ViewportPointToRay(ViewportCenter);

        Transform currentHover = null;

        if (Physics.Raycast(ray, out var hit, range, interactableLayer))
        {
            currentHover = hit.transform;
        }

        HandleInteraction(currentHover);
    }

    void HandleInteraction(Transform currentHover)
    {
        if (hovered?.transform == currentHover) return;

        if (currentHover == null)
            hovered = null;
        else
            { currentHover.TryGetComponent(out hovered); print("GETCOMPONENT"); }

        text.text = hovered?.GetText() ?? "";
    }
}