using NaughtyAttributes;
using UnityEngine;

namespace Recounter.Store
{
    public class SavableTransform : MonoBehaviour
    {
        [SerializeField] bool _auto = true;
        [SerializeField, ShowIf(nameof(_auto))] bool _position;
        [SerializeField, ShowIf(nameof(_auto))] bool _rotation;

        void Awake()
        {
            GameManager.StoreData.PreSave += OnSave;
        }

        void OnDestroy()
        {
            GameManager.StoreData.PreSave -= OnSave;
        }

        void OnSave()
        {
            if (!_auto) return;

            Save(_position, _rotation);
        }

        public void Save(bool position, bool rotation)
        {
            if (_position) GameManager.StoreData.SetKey(name + " pos", transform.position);
            if (_rotation) GameManager.StoreData.SetKey(name + " rot", transform.eulerAngles);
        }

        void Restore()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            RigidbodyInterpolation temp = default;

            if (rigidbody)
            {
                temp = rigidbody.interpolation;
                rigidbody.interpolation = RigidbodyInterpolation.None;
            }

            if (_position)
            {
                if (GameManager.StoreData.TryGetKey(name + " pos", out Vector3 pos))
                    transform.position = pos;
            }

            if (_rotation)
            {
                if (GameManager.StoreData.TryGetKey(name + " rot", out Vector3 rot))
                    transform.eulerAngles = rot;
            }

            if (rigidbody)
            {
                rigidbody.interpolation = temp;
            }
        }

        void Start() => Restore();
    }
}
