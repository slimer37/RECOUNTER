using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter.Tablet
{
    public class Tablet : MonoBehaviour
    {
        [SerializeField] Employee _employee;

        [SerializeField] Canvas _canvas;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] float _canvasFadeTime;
        [SerializeField] PlayerController _controller;
        [SerializeField] PlayerInteraction _interaction;

        [Header("Hands Full Check")]
        [SerializeField] float _messageExpiry;
        [SerializeField] float _messageFadeTime;
        [SerializeField] string _handsFullMessage;

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
            _canvas.enabled = false;

            _openTabletAction = InputLayer.Tablet.OpenTablet;
            _openTabletAction.performed += _ => OpenTablet();

            _loweredPosition = _physicalTablet.localPosition;
            _loweredRotation = _physicalTablet.eulerAngles;
        }

        void OpenTablet()
        {
            if (Pause.IsPaused || _currentTween.IsActive() && _currentTween.IsPlaying()) return;

            CancelInvoke();

            if (!_employee.HandsAreFree)
            {
                _employee.ShowMessage(_handsFullMessage, _messageExpiry, _messageFadeTime);
                return;
            }

            _isUp = !_isUp;

            _currentTween = DOTween.Sequence()
                .Append(_physicalTablet.DOLocalMove(_isUp ? _raisedPosition : _loweredPosition, _raiseTime).SetEase(_ease))
                .Join(_physicalTablet.DOLocalRotate(_isUp ? _raisedRotation : _loweredRotation, _raiseTime).SetEase(_ease));

            if (_isUp)
            {
                ConfigurePlayerComponents(true);

                _currentTween.OnComplete(() => ShowUI(true));
            }
            else
            {
                ConfigurePlayerComponents(false);
                ShowUI(false);
            }
        }

        void ConfigurePlayerComponents(bool tabletOpen)
        {
            InputLayer.Suspend(tabletOpen, true);
            _interaction.Suspend(tabletOpen);
            _employee.ShowHud(!tabletOpen);
        }

        void ShowUI(bool show)
        {
            _canvas.enabled = true;

            _canvasGroup.DOKill();

            var tween = _canvasGroup.DOFade(show ? 1 : 0, _canvasFadeTime);

            if (show)
            {
                // Reset message
                _employee.ClearMessage();

                return;
            }

            tween.OnComplete(() => _canvas.enabled = false);
        }
    }
}
