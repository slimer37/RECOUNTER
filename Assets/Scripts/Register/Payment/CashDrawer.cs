using DG.Tweening;
using FMODUnity;
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

        [Header("SFX")]
        [SerializeField] EventReference _openSfx;
        [SerializeField] EventReference _closeSfx;

        public bool IsAnimating => _drawerTween.IsActive() && _drawerTween.IsPlaying();

        Tween _drawerTween;

        public bool IsOpen { get; private set; }

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = IsOpen ? Icon.Push : Icon.Pull,
            text = IsOpen ? "Close" : "Open"
        };

        protected override HudInfo FormNonInteractHud(Employee e) => new()
        {
            icon = Icon.Invalid
        };

        protected override bool CanInteract(Employee e) => !IsAnimating && (_openableByHand || IsOpen);

        protected override void OnInteract(Employee e)
        {
            SetOpen(!IsOpen);
        }

        public void Open() => SetOpen(true);

        void SetOpen(bool open)
        {
            if (IsAnimating)
            {
                throw new InvalidOperationException("Drawer is animating.");
            }

            if (IsOpen == open)
            {
                Debug.LogWarning($"Drawer is already {(open ? "open" : "closed")}.");
                return;
            }

            _drawerTween = _drawer.DOLocalMove(
                open ? _openPosition : _closedPosition,
                open ? _openTime : _closeTime)
                .SetEase(open ? _openEase : _closeEase);

            if (open)
            {
                IsOpen = true;
                RuntimeManager.PlayOneShotAttached(_openSfx, _drawer.gameObject);
            }
            else
            {
                _drawerTween.OnComplete(() => IsOpen = false);
                RuntimeManager.PlayOneShotAttached(_closeSfx, _drawer.gameObject);
            }
        }
    }
}
