using Recounter;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] Employee employee;

    [SerializeField] float range;
    [SerializeField] Camera cam;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] LayerMask raycastMask;
    [SerializeField] LayerMask interactableMask;

    [Header("Fade Reticle")]
    [SerializeField] float detectionRadius;
    [SerializeField] float fade;
    [SerializeField] CanvasGroup fadeReticle;

    [Header("Animation")]
    [SerializeField] float punchAmount;
    [SerializeField] float punchDuration;

    [Header("Icons")]
    [SerializeField] Image iconImage;
    [SerializeField] InteractableIconSettings iconSettings;

    Interactable hovered;
    Interactable interactTarget;

    Transform lastHoverTarget;

    Tween punch;

    float targetAlpha;

    bool waitingToCancelInteract;

    void Awake()
    {
        var interactAction = InputLayer.Interaction.Interact;
        interactAction.performed += OnInteract;
        interactAction.canceled += OnInteractCancel;

        punch = iconImage.rectTransform.DOPunchScale(Vector3.one * punchAmount, punchDuration)
            .Pause().SetAutoKill(false);

        Pause.Paused += OnPaused;
    }

    void OnPaused(bool paused)
    {
        if (paused || !waitingToCancelInteract) return;

        CancelInteract();

        waitingToCancelInteract = false;
    }

    void ResetUI()
    {
        text.text = "";
        iconImage.sprite = iconSettings.GetSprite(Interactable.Icon.None);
        fadeReticle.alpha = 1;
    }

    void OnEnable()
    {
        ResetUI();
    }

    void OnDisable()
    {
        if (!hovered || !text || !iconImage) return;
        ResetUI();
        HandleInteraction(null);
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!hovered || Pause.IsPaused) return;

        interactTarget = hovered;
        interactTarget.Interact(employee);
    }

    void OnInteractCancel(InputAction.CallbackContext context)
    {
        if (!interactTarget) return;

        if (Pause.IsPaused)
        {
            waitingToCancelInteract = true;
            return;
        }

        CancelInteract();
    }
    
    void CancelInteract()
    {
        if (!interactTarget) return;

        interactTarget.EndInteract();
        interactTarget = null;
    }

    void LateUpdate()
    {
        if (Pause.IsPaused) return;

        if (hovered)
        {
            UpdateUI();
        }

        Transform currentHover = null;

        if (Physics.Raycast(cam.ViewportPointToRay(Vector2.one / 2), out var hit, range, raycastMask))
        {
            if (interactableMask == (interactableMask | (1 << hit.transform.gameObject.layer)))
            {
                currentHover = hit.collider.transform;
            }
        }

        HandleInteraction(currentHover);
    }

    void Update()
    {
        fadeReticle.alpha = Mathf.Lerp(fadeReticle.alpha, targetAlpha, fade * Time.deltaTime);
    }

    void FixedUpdate()
    {
        targetAlpha = 0;

        if (Physics.CheckSphere(cam.transform.position, detectionRadius, interactableMask))
        {
            targetAlpha = 1;
        }
    }

    void HandleInteraction(Transform currentHover)
    {
        if (lastHoverTarget == currentHover) return;

        lastHoverTarget = currentHover;
        hovered?.OnHover(false);

        if (currentHover)
        {
            hovered = currentHover.GetComponentInParent<Interactable>();
            hovered.OnHover(true);

            UpdateUI(true);
        }
        else
        {
            hovered = null;

            ResetUI();
        }
    }

    void UpdateUI(bool forcePunch = false)
    {
        var info = hovered.GetHud(employee);
        var iconSprite = iconSettings.GetSprite(info.icon);

        // Punch when icon changes (except if it's the blank pointer).
        if (info.icon != Interactable.Icon.None && (forcePunch || iconImage.sprite != iconSprite || text.text != info.text))
            punch.Restart();

        text.text = info.text;
        iconImage.sprite = iconSprite;
    }
}