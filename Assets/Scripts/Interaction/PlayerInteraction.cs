using Recounter;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] Employee _employee;

    [SerializeField] float _range;
    [SerializeField] Camera _cam;
    [SerializeField] LayerMask _raycastMask;
    [SerializeField] LayerMask _interactableMask;
    [SerializeField] InteractionReticle _reticle;

    Interactable _hovered;
    Interactable _interactTarget;

    Transform _lastHoverTarget;

    bool _waitingToCancelInteract;

    void Awake()
    {
        var interactAction = InputLayer.Interaction.Interact;
        interactAction.performed += OnInteract;
        interactAction.canceled += OnInteractCancel;

        Pause.Paused += OnPaused;
    }

    void OnPaused(bool paused)
    {
        if (paused || !_waitingToCancelInteract) return;

        CancelInteract();

        _waitingToCancelInteract = false;
    }

    void OnDisable() => HandleHoverTarget(null);

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!_hovered || Pause.IsPaused) return;

        _interactTarget = _hovered;
        _interactTarget.Interact(_employee);
    }

    void OnInteractCancel(InputAction.CallbackContext context)
    {
        if (!_interactTarget) return;

        if (Pause.IsPaused)
        {
            _waitingToCancelInteract = true;
            return;
        }

        CancelInteract();
    }

    void CancelInteract()
    {
        if (!_interactTarget) return;

        _interactTarget.EndInteract();
        _interactTarget = null;
    }

    void LateUpdate()
    {
        if (Pause.IsPaused) return;

        if (_hovered)
        {
            UpdateReticle();
        }

        Transform currentHover = null;

        if (Physics.Raycast(_cam.ViewportPointToRay(Vector2.one / 2), out var hit, _range, _raycastMask))
        {
            if (_interactableMask == (_interactableMask | (1 << hit.transform.gameObject.layer)))
            {
                currentHover = hit.collider.transform;
            }
        }

        HandleHoverTarget(currentHover);
    }

    void UpdateReticle(bool forcePunch = false) => _reticle.UpdateUI(_hovered.GetHud(_employee), forcePunch);

    void ClearReticle() => _reticle.Clear();

    void HandleHoverTarget(Transform currentHover)
    {
        if (_lastHoverTarget == currentHover) return;

        _lastHoverTarget = currentHover;
        _hovered?.OnExitHover(_employee);

        if (currentHover)
        {
            _hovered = currentHover.GetComponentInParent<Interactable>();
            _hovered.OnEnterHover(_employee);

            UpdateReticle(true);
        }
        else
        {
            _hovered = null;

            ClearReticle();
        }
    }
}
