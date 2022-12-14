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
    [SerializeField] Sprite[] icons;

    Interactable hovered;
    Interactable interactTarget;

    Tween punch;
    Controls.PlayerActions playerControls;

    void Awake()
    {
        if (icons.Length != System.Enum.GetNames(typeof(Interactable.Icon)).Length)
            Debug.LogError("Wrong number of interaction icons assigned.", this);

        playerControls = new Controls().Player;
        playerControls.Interact.performed += OnInteract;
        playerControls.Interact.canceled += OnInteractCancel;

        punch = iconImage.rectTransform.DOPunchScale(Vector3.one * punchAmount, punchDuration)
            .Pause().SetAutoKill(false);
    }

    void ResetUI()
    {
        text.text = "";
        iconImage.sprite = icons[0];
    }

    void OnEnable()
    {
        ResetUI();
        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
        if (!hovered || !text || !iconImage) return;
        HandleInteraction(null);
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!hovered) return;

        interactTarget = hovered;
        interactTarget.Interact(employee);
    }

    void OnInteractCancel(InputAction.CallbackContext context)
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

        if (Physics.Raycast(cam.ViewportPointToRay(Vector2.one / 2), out var hit, range, interactableMask))
        {
            currentHover = hit.transform;
        }

        HandleInteraction(currentHover);
    }

    void FixedUpdate()
    {
        var alpha = 0;

        if (Physics.CheckSphere(cam.transform.position, detectionRadius, interactableMask))
        {
            alpha = 1;
        }

        fadeReticle.alpha = Mathf.Lerp(fadeReticle.alpha, alpha, fade * Time.fixedDeltaTime);
    }

    void HandleInteraction(Transform currentHover)
    {
        if (hovered?.transform == currentHover) return;

        hovered?.OnHover(false);

        if (currentHover)
        {
            hovered = currentHover.GetComponent<Interactable>();
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
        var info = hovered.GetHudInfo(employee);
        var icon = icons[(int)info.icon];

        // Punch when icon changes (except if it's the blank pointer).
        if (info.icon != Interactable.Icon.None && (forcePunch || iconImage.sprite != icon))
            punch.Restart();

        text.text = info.text;
        iconImage.sprite = icon;
    }
}