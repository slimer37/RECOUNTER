using Recounter;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour, IHoverHandler<Interactable>
{
    [SerializeField] Employee _employee;

    [SerializeField] float _range;
    [SerializeField] Camera _cam;
    [SerializeField] LayerMask _raycastMask;
    [SerializeField] LayerMask _interactableMask;
    [SerializeField] InteractionReticle _reticle;

    Interactable _interactTarget;

    bool _waitingToCancelInteract;

    HoverRaycaster<Interactable> _raycaster;

    void Awake()
    {
        var interactAction = InputLayer.Interaction.Interact;
        interactAction.performed += OnInteract;
        interactAction.canceled += OnInteractCancel;

        Pause.Paused += OnPaused;

        _raycaster = new(_cam, _range, _raycastMask, _interactableMask, GetComponentType.InParent, this);
    }

    void OnPaused(bool paused)
    {
        if (paused || !_waitingToCancelInteract) return;

        CancelInteract();

        _waitingToCancelInteract = false;
    }

    public void Suspend(bool suspend)
    {
        enabled = !suspend;
        _reticle.EnableFade(!suspend);

        if (suspend && !_raycaster.HoverTarget)
        {
            _raycaster.Clear();
        }
        else
        {
            _reticle.Clear();
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!_raycaster.HoverTarget || Pause.IsPaused) return;

        _interactTarget = _raycaster.HoverTarget;
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

        _raycaster.Raycast();
    }

    void UpdateReticle(bool forcePunch = false) => _reticle.UpdateUI(_raycaster.HoverTarget.GetHud(_employee), forcePunch);

    public void HoverEnter(Interactable obj)
    {
        obj.OnEnterHover(_employee);

        UpdateReticle(true);
    }

    public void HoverStay(Interactable obj)
    {
        UpdateReticle();
    }

    public void HoverExit(Interactable obj)
    {
        obj.OnExitHover(_employee);
        _reticle.Clear();
    }
}
