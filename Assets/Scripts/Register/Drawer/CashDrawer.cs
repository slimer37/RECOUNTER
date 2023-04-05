using DG.Tweening;
using System;
using UnityEngine;

namespace Recounter.Service
{
    public class CashDrawer : Interactable
    {
        [SerializeField] bool _openableByHand;

        [Header("Animation")]
        [SerializeField] Transform _drawer;
        [SerializeField] Vector3 _closedPosition;
        [SerializeField] Vector3 _openPosition;
        [SerializeField] float _openTime;
        [SerializeField] Ease _openEase;
        [SerializeField] float _closeTime;
        [SerializeField] Ease _closeEase;

        public bool IsAnimating => _drawerTween.IsActive() && _drawerTween.IsPlaying();

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

        protected override bool CanInteract(Employee e) => !IsAnimating && (_openableByHand || _isOpen);

        protected override void OnInteract(Employee e)
        {
            SetOpen(!_isOpen);
        }

        public void Open() => SetOpen(true);

        void SetOpen(bool open)
        {
            if (IsAnimating)
            {
                throw new InvalidOperationException("Drawer is animating.");
            }

            if (_isOpen == open)
            {
                Debug.LogWarning($"Drawer is already {(open ? "open" : "closed")}.");
                return;
            }

            _isOpen = open;

            _drawerTween = _drawer.DOLocalMove(
                _isOpen ? _openPosition : _closedPosition,
                _isOpen ? _openTime : _closeTime)
                .SetEase(_isOpen ? _openEase : _closeEase);
        }
    }
}
