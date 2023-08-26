using DG.Tweening;
using UnityEngine;

namespace Recounter
{
    public class HandTruck : Tool
    {
        [SerializeField] Transform _layPoint;
        [SerializeField] float _dropSpeed;
        [SerializeField] Ease _dropEase;
        [SerializeField] float _raiseSpeed;
        [SerializeField] Ease _raiseEase;
        [SerializeField] Transform _visual;
        [SerializeField] Vehicle _vehicle;

        float _raisedAngle;

        Tween _tween;

        protected override void Awake()
        {
            base.Awake();

            _raisedAngle = _visual.eulerAngles.x;

            Drop();
        }

        void Raise()
        {
            _tween?.Kill();
            _tween = _visual.DOLocalRotate(Vector3.right * _raisedAngle, _raiseSpeed).SetEase(_raiseEase).SetSpeedBased();
        }

        void Drop()
        {
            _tween?.Kill();
            _tween = _visual.DOLocalRotate(Vector3.zero, _dropSpeed).SetEase(_dropEase).SetSpeedBased().OnKill(() => _vehicle.Locked = false);

            _vehicle.Locked = true;
        }

        protected override void OnEquip() => Raise();
        protected override void OnUnequip() => Drop();
    }
}
