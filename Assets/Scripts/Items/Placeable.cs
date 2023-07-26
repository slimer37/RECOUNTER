using UnityEngine;
using NaughtyAttributes;

namespace Recounter.Items
{
    public class Placeable : Item
    {
        [SerializeField] Renderer rend;
        [SerializeField] bool isCylindrical;

        [Header("Optional")]
        [SerializeField] Rigidbody rb;
        [SerializeField, EnableIf(nameof(HasRigidbody)), AllowNesting] bool isThrowable = true;

        [Header("Bounds Override")]
        [SerializeField] bool overridesBounds;
        [SerializeField, EnableIf(nameof(overridesBounds)), AllowNesting] Vector3 overrideCenter;
        [SerializeField, EnableIf(nameof(overridesBounds)), AllowNesting, Min(0)] Vector3 overrideSize;

        Collider[] colliders;

        public Vector3 OriginShift => GetOriginShift();

        public bool IsThrowable => rb && isThrowable;
        public bool HasRigidbody => rb;

        bool _justReleased;

        public static LayerMask DefaultIntersectionMask;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (overridesBounds)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawWireCube(overrideCenter, overrideSize);
            }
            else if (rend)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(rend.localBounds.center, rend.localBounds.size);
            }
        }

        void Reset()
        {
            TryGetComponent(out rend);
            TryGetComponent(out rb);
        }

        void OnValidate()
        {
            if (!rb && isThrowable)
            {
                Debug.LogWarning($"Cannot set {nameof(isThrowable)} if no rigidbody is selected.");
                isThrowable = false;
            }
        }

        void Awake()
        {
            if (DefaultIntersectionMask == default)
            {
                DefaultIntersectionMask = LayerMask.GetMask("Default", "Interactable", "Player");
            }

            colliders = GetComponentsInChildren<Collider>();
        }

        public void Throw(Vector3 force)
        {
            Release();
            rb.AddForce(force, ForceMode.Impulse);
        }

        Vector3 GetScaledExtents() => overridesBounds ?
            overrideSize / 2 : Vector3.Scale(transform.lossyScale, rend.localBounds.extents);

        Vector3 GetOriginShift() => overridesBounds ?
            overrideCenter : rend.localBounds.center;

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

            if (intersects && isCylindrical)
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

            if (isCylindrical)
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

            if (rb)
                rb.isKinematic = true;
        }

        protected override void OnRelease()
        {
            _justReleased = true;

            EnableColliders(true);

            if (rb)
            {
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_justReleased)
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        void EnableColliders(bool enable)
        {
            foreach (var col in colliders)
            {
                col.enabled = enable;
            }
        }
    }
}