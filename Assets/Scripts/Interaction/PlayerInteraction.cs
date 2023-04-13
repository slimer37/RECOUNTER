using Recounter;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] Employee _employee;

    [SerializeField] float _range;
    [SerializeField] Camera _cam;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] LayerMask _raycastMask;
    [SerializeField] LayerMask _interactableMask;

    [Header("Fade Reticle")]
    [SerializeField] float _detectionRadius;
    [SerializeField] float _fade;
    [SerializeField] CanvasGroup _fadeReticle;

    [Header("Animation")]
    [SerializeField] float _punchAmount;
    [SerializeField] float _punchDuration;

    [Header("Icons")]
    [SerializeField] Image _fillBar;
    [SerializeField] Image _iconImage;
    [SerializeField] InteractableIconSettings _iconSettings;

    Interactable _hovered;
    Interactable _interactTarget;

    Transform _lastHoverTarget;

    Tween _punch;

    float _targetAlpha;

    bool _waitingToCancelInteract;

    void Awake()
    {
        var interactAction = InputLayer.Interaction.Interact;
        interactAction.performed += OnInteract;
        interactAction.canceled += OnInteractCancel;

        _punch = _iconImage.rectTransform.DOPunchScale(Vector3.one * _punchAmount, _punchDuration)
            .Pause().SetAutoKill(false);

        Pause.Paused += OnPaused;
    }

    void OnPaused(bool paused)
    {
        if (paused || !_waitingToCancelInteract) return;

        CancelInteract();

        _waitingToCancelInteract = false;
    }

    void ResetUI()
    {
        _text.text = "";
        _iconImage.sprite = _iconSettings.GetSprite(Interactable.Icon.None);
        _fadeReticle.alpha = 1;
        _fillBar.fillAmount = 0;
    }

    void OnEnable()
    {
        ResetUI();
    }

    void OnDisable()
    {
        if (!_text || !_iconImage) return;

        ResetUI();

        if (!_hovered) return;

        HandleInteraction(null);
    }

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
            UpdateUI();
        }

        Transform currentHover = null;

        if (Physics.Raycast(_cam.ViewportPointToRay(Vector2.one / 2), out var hit, _range, _raycastMask))
        {
            if (_interactableMask == (_interactableMask | (1 << hit.transform.gameObject.layer)))
            {
                currentHover = hit.collider.transform;
            }
        }

        HandleInteraction(currentHover);
    }

    void Update()
    {
        _fadeReticle.alpha = Mathf.Lerp(_fadeReticle.alpha, _targetAlpha, _fade * Time.deltaTime);
    }

    void FixedUpdate()
    {
        _targetAlpha = 0;

        if (Physics.CheckSphere(_cam.transform.position, _detectionRadius, _interactableMask))
        {
            _targetAlpha = 1;
        }
    }

    void HandleInteraction(Transform currentHover)
    {
        if (_lastHoverTarget == currentHover) return;

        _lastHoverTarget = currentHover;
        _hovered?.OnHover(false);

        if (currentHover)
        {
            _hovered = currentHover.GetComponentInParent<Interactable>();
            _hovered.OnHover(true);

            UpdateUI(true);
        }
        else
        {
            _hovered = null;

            ResetUI();
        }
    }

    void UpdateUI(bool forcePunch = false)
    {
        var info = _hovered.GetHud(_employee);
        var iconSprite = _iconSettings.GetSprite(info.icon);
        var fill = info.fill ?? 0;

        // Punch when icon changes (except if it's the blank pointer).
        if (info.icon != Interactable.Icon.None && (forcePunch || _iconImage.sprite != iconSprite || _text.text != info.text))
            _punch.Restart();

        _text.text = info.text;
        _iconImage.sprite = iconSprite;
        _fillBar.fillAmount = fill;
    }
}