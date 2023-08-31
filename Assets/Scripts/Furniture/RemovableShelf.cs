using UnityEngine;

namespace Recounter
{
    public class RemovableShelf : Interactable
    {
        [SerializeField] Vector3 _holdPos;
        [SerializeField] Vector3 _holdRot;
        [SerializeField] RemovedShelfTool _removedShelfTool;
        [SerializeField] BoxCollider _collider;

        Collider[] _colliders;

        void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>();
        }

        void SetColliders(bool enabled)
        {
            foreach (var col in _colliders)
            {
                col.enabled = enabled;
            }
        }

        protected override HudInfo FormHud(Employee e) => new()
        {
            text = "Remove shelf",
            icon = Icon.Pickup
        };

        protected override void OnInteract(Employee e)
        {
            LastInteractor.LeftHand.Hold(this, _holdPos, Quaternion.Euler(_holdRot));
            _removedShelfTool.Equip(e);
            SetColliders(false);
        }

        public bool Intersects()
        {
            return Physics.CheckBox(transform.TransformPoint(_collider.center), _collider.size / 2, transform.rotation);
        }

        public void AttachToShelf()
        {
            _removedShelfTool.Unequip();
            SetColliders(true);
        }

        protected override bool CanInteract(Employee e) =>
            e.RightHand.IsFull && e.RightHand.HeldObject.CompareTag("Shelf Tool");
    }
}
