using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] Transform cam;
    [SerializeField] TextMeshProUGUI text;

    [Header("Animation")]
    [SerializeField] float punchAmount;
    [SerializeField] float punchDuration;

    [Header("Icons")]
    [SerializeField] Image iconImage;
    [SerializeField] Sprite[] icons;

    Interactable hovered;
    Tween punch;
    Controls.PlayerActions playerControls;

    void Awake()
    {
        if (icons.Length != System.Enum.GetNames(typeof(Interactable.Icon)).Length)
            Debug.LogError("Wrong number of interaction icons assigned.", this);

        playerControls = new Controls().Player;
        playerControls.Interact.performed += OnInteract;

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
        if (!hovered) return;
        HandleInteraction(null);
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!hovered) return;

        if (!hovered.CanInteract()) return;

        hovered.Interact();
    }

    void LateUpdate()
    {
        if (Pause.IsPaused) return;

        if (hovered)
        {
            UpdateUI();
        }

        Transform currentHover = null;

        if (Physics.Raycast(cam.position, cam.forward, out var hit, range))
        {
            currentHover = hit.transform;
        }

        HandleInteraction(currentHover);
    }

    void HandleInteraction(Transform currentHover)
    {
        if (hovered?.transform == currentHover) return;

        if (currentHover && currentHover.TryGetComponent(out hovered))
        {
            hovered?.OnHover(true);
            punch.Restart();
        }
        else
        {
            hovered?.OnHover(false);
            hovered = null;

            ResetUI();
        }
    }

    void UpdateUI()
    {
        text.text = hovered.GetText();
        iconImage.sprite = icons[(int)hovered.GetIcon()];
    }
}