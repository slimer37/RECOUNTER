using UnityEngine;

namespace Recounter.Store
{
    public class SavableTransform : MonoBehaviour
    {
        [SerializeField] bool _position;
        [SerializeField] bool _rotation;
        [SerializeField] bool _writeOnPreSave = true;

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
            if (!_writeOnPreSave) return;

            Save();
        }

        public void Save()
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
