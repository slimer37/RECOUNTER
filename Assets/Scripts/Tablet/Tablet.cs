using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tablet : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] PlayerController _controller;
    [SerializeField] PlayerInteraction _interaction;

    [Header("Hands Full Check")]
    [SerializeField] Employee _employee;
    [SerializeField] Canvas _messageCanvas;
    [SerializeField] CanvasGroup _messageUI;
    [SerializeField] float _messageExpiry;
    [SerializeField] float _fadeSpeed;

    [Header("Physical Tablet")]
    [SerializeField] Transform _physicalTablet;

    [Header("Raise Animation")]
    [SerializeField] float _raiseTime;
    [SerializeField] Vector3 _raisedPosition;
    [SerializeField] Vector3 _raisedRotation;
    [SerializeField] Ease _ease;

    Tween _currentTween;

    bool _isUp;
    InputAction _openTabletAction;

    Vector3 _loweredPosition;
    Vector3 _loweredRotation;

    void Awake()
    {
        _messageCanvas.enabled = false;

        _openTabletAction = new Controls().Player.OpenTablet;
        _openTabletAction.performed += _ => OpenTablet();

        _loweredPosition = _physicalTablet.localPosition;
        _loweredRotation = _physicalTablet.eulerAngles;
    }

    void OnEnable() => _openTabletAction.Enable();
    void OnDisable() => _openTabletAction.Disable();

    void OpenTablet()
    {
        if (_currentTween.IsActive() && _currentTween.IsPlaying()) return;

        CancelInvoke();

        if (!_employee.HandsAreFree)
        {
            _messageCanvas.enabled = true;
            _messageUI.alpha = 1;
            GetFadeTween().SetDelay(_messageExpiry);
            return;
        }

        GetFadeTween();

        _isUp = !_isUp;

        //_canvas.enabled = _isUp;

        _currentTween = DOTween.Sequence()
            .Append(_physicalTablet.DOLocalMove(_isUp ? _raisedPosition : _loweredPosition, _raiseTime).SetEase(_ease))
            .Join(_physicalTablet.DOLocalRotate(_isUp ? _raisedRotation : _loweredRotation, _raiseTime).SetEase(_ease));

        if (_isUp)
        {
            ConfigurePlayerComponents(false);
        }
        else
        {
            _currentTween.OnComplete(() =>
            {
                ConfigurePlayerComponents(true);
            });
        }
    }

    void ConfigurePlayerComponents(bool enabled)
    {
        _controller.Suspend(!enabled);
        _interaction.enabled = enabled;
        _employee.ShowHud(enabled);
    }

    Tween GetFadeTween()
    {
        _messageUI.DOKill();
        return _messageUI.DOFade(0, _fadeSpeed)
            .SetSpeedBased()
            .OnComplete(() => _messageCanvas.enabled = false);
    }
}
