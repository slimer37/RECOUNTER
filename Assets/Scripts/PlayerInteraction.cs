using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] Transform cam;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] LayerMask interactableLayer;

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

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!hovered) return;

        if (!hovered.CanInteract()) return;

        hovered.Interact();
    }

    void Update()
    {
        if (hovered)
        {
            text.text = hovered.GetText();
        }

        Transform currentHover = null;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range, interactableLayer))
        {
            currentHover = hit.transform;
        }

        HandleInteraction(currentHover);
    }

    void HandleInteraction(Transform currentHover)
    {
        if (hovered?.transform == currentHover) return;

        if (currentHover)
        {
            hovered = currentHover.GetComponent<Interactable>();
            hovered.OnHover(true);
        }
        else
        {
            hovered.OnHover(false);
            text.text = "";
            hovered = null;
        }
    }
}