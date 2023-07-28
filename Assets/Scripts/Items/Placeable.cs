using UnityEngine;
using NaughtyAttributes;

namespace Recounter.Items
{
    public class Placeable : Item
    {
        [SerializeField] Renderer _rend;
        [SerializeField] bool _isCylindrical;

        [Header("Optional")]
        [SerializeField] Rigidbody _rb;
        [SerializeField, EnableIf(nameof(HasRigidbody)), AllowNesting] bool _isThrowable = true;

        [Header("Bounds Override")]
        [SerializeField] bool _overridesBounds;
        [SerializeField, EnableIf(nameof(_overridesBounds)), AllowNesting] Vector3 _overrideCenter;
        [SerializeField, EnableIf(nameof(_overridesBounds)), AllowNesting, Min(0)] Vector3 _overrideSize;

        Collider[] _colliders;

        public Vector3 OriginShift => GetOriginShift();

        public bool IsThrowable => _rb && _isThrowable;
        public bool HasRigidbody => _rb;

        bool _justReleased;

        public static LayerMask DefaultIntersectionMask;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (_overridesBounds)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireCube(_overrideCenter, _overrideSize);
            }
            else if (_rend)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_rend.localBounds.center, _rend.localBounds.size);
            }
        }

        void Reset()
        {
            TryGetComponent(out _rend);
            TryGetComponent(out _rb);
        }

        void OnValidate()
        {
            if (!_rb && _isThrowable)
            {
                Debug.LogWarning($"Cannot set {nameof(_isThrowable)} if no rigidbody is selected.", this);
                _isThrowable = false;
            }
        }

        void Awake()
        {
            if (DefaultIntersectionMask == default)
            {
                DefaultIntersectionMask = LayerMask.GetMask("Default", "Interactable", "Player");
            }

            _colliders = GetComponentsInChildren<Collider>();
        }

        public void Throw(Vector3 force)
        {
            Release();
            _rb.AddForce(force, ForceMode.Impulse);
        }

        Vector3 GetScaledExtents() => _overridesBounds ?
            _overrideSize / 2 : Vector3.Scale(transform.lossyScale, _rend.localBounds.extents);

        Vector3 GetOriginShift() => _overridesBounds ?
            _overrideCenter : _rend.localBounds.center;

        public bool IsIntersecting() => IsIntersecting(DefaultIntersectionMask);

        public bool IsIntersecting(LayerMask mask) =>
            WouldIntersectAt(transform.position, transform.rotation, mask);

        public bool WouldIntersectAt(Vector3 position, Quaternion rotation) =>
            WouldIntersectAt(position, rotation, DefaultIntersectionMask);

        public bool WouldIntersectAt(Vector3 position, Quaternion rotation, LayerMask mask)
        {
            var scaledExtents = GetScaledExtents();
            position += Quaternion.Inverse(rotation) * GetOriginShift();

            var intersects = Physics.CheckBox(position, scaledExtents, rotation, mask);

            if (intersects && _isCylindrical)
            {
                var radius = Mathf.Max(scaledExtents.x, scaledExtents.z);
                intersects = Physics.CheckCapsule(position - Vector3.up * scaledExtents.y, position + Vector3.up * scaledExtents.y, radius, mask);
            }

            return intersects;
        }

        public float SizeAlong(Vector3 localDirection)
        {
            var scaledExtents = GetScaledExtents();
            var originShift = Vector3.Dot(localDirection, GetOriginShift());

            if (_isCylindrical)
            {
                var radialComponent = new Vector2(localDirection.x, localDirection.z).magnitude;
                return originShift + Mathf.Max(scaledExtents.x, scaledExtents.z) * radialComponent + scaledExtents.y * Mathf.Abs(localDirection.y);
            }
            else
            {
                var absDirection = new Vector3(Mathf.Abs(localDirection.x), Mathf.Abs(localDirection.y), Mathf.Abs(localDirection.z));
                return originShift + Vector3.Dot(absDirection, scaledExtents);
            }
        }

        protected override HudInfo FormHud(Employee e) => new() { icon = Icon.Pickup, text = "Pick up" };

        protected override bool CanInteract(Employee e) => !IsHeld && !e.ItemHotbar.IsActiveSlotFull;

        protected override void OnInteract(Employee e)
        {
            e.ItemHotbar.TryAddItem(this);
        }

        protected override void OnPickUp()
        {
            EnableColliders(false);

            if (_rb)
                _rb.isKinematic = true;
        }

        protected override void OnRelease()
        {
            _justReleased = true;

            EnableColliders(true);

            if (_rb)
            {
                _rb.isKinematic = false;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_justReleased)
                _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        void EnableColliders(bool enable)
        {
            foreach (var col in _colliders)
            {
                col.enabled = enable;
            }
        }
    }
}