using DG.Tweening;
using UnityEngine;

namespace Recounter.Service
{
    public class CashDrawer : Interactable
    {
        [Header("Animation")]
        [SerializeField] Transform _drawer;
        [SerializeField] Vector3 _closedPosition;
        [SerializeField] Vector3 _openPosition;
        [SerializeField] float _openTime;
        [SerializeField] Ease _openEase;
        [SerializeField] float _closeTime;
        [SerializeField] Ease _closeEase;

        Tween _drawerTween;

        bool _isOpen;

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = _isOpen ? Icon.Push : Icon.Pull,
            text = _isOpen ? "Close" : "Open"
        };

        protected override HudInfo FormNonInteractHud(Employee e) => new()
        {
            icon = Icon.Invalid
        };

        protected override bool CanInteract(Employee e) => !_drawerTween.IsActive() || !_drawerTween.IsPlaying();

        protected override void OnInteract(Employee e)
        {
            _isOpen = !_isOpen;

            _drawerTween = _drawer.DOLocalMove(
                _isOpen ? _openPosition : _closedPosition,
                _isOpen ? _openTime : _closeTime)
                .SetEase(_isOpen ? _openEase : _closeEase);
        }
    }
}
