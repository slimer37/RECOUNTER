using DG.Tweening;
using UnityEngine;

namespace Recounter
{
    public class HandTruck : Vehicle
    {
        [Header("Hand Truck")]
        [SerializeField] Transform _layPoint;
        [SerializeField] float _filledSpeed;
        [SerializeField] FurniturePlacement _placement;

        [Header("Animation")]
        [SerializeField] float _dropSpeed;
        [SerializeField] Ease _dropEase;
        [SerializeField] float _raiseSpeed;
        [SerializeField] Ease _raiseEase;
        [SerializeField] Transform _visual;

        float _raisedAngle;

        Furniture _load;

        Furniture _target;

        Tween _tween;

        protected override float Speed => _load ? _filledSpeed : _defaultSpeed;

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
            _tween = _visual.DOLocalRotate(Vector3.zero, _dropSpeed).SetEase(_dropEase).SetSpeedBased().OnKill(() => Locked = false);

            Locked = true;
        }

        protected override HudInfo FormHud(Employee e) => new()
        {
            text = "Push",
            icon = Icon.HandTruck
        };

        public void BeginFurniturePlacement(Employee e)
        {
            _placement.Activate(e, _load, () =>
            {
                _load.transform.SetParent(null);
                _load = null;
            });
        }

        protected override void OnStartedBeingPushed()
        {
            Raise();

            _target?.Highlight();
        }

        protected override void OnStoppedBeingPushed()
        {
            Drop();

            _target?.RemoveHighlight();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsBeingPushed || _load || !other.CompareTag("Furniture")) return;

            _target?.RemoveHighlight();
            _target = other.GetComponentInParent<Furniture>();
            _target.Highlight();
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsBeingPushed || _load || !other.CompareTag("Furniture")) return;

            if (_target == other.GetComponentInParent<Furniture>())
            {
                _target.RemoveHighlight();
                _target = null;
            }
        }

        public void LoadFurniture()
        {
            _load = _target;

            _load.LoadToMover(this);

            _target.RemoveHighlight();

            _load.transform.SetParent(_layPoint);

            _load.transform.rotation = _layPoint.rotation;

            _load.transform.localPosition = Vector3.Scale(_load.Extents - _load.CenterOffset, new Vector3(0, 1, 1));

            _target = null;
        }

        protected override void PushingUpdate()
        {
            if (!_target) return;

            if (InputLayer.Placement.Place.WasPressedThisFrame())
            {
                if (!_load)
                {
                    LoadFurniture();
                }
            }
        }
    }
}
