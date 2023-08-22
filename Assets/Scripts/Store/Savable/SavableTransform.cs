using UnityEngine;

namespace Recounter.Store
{
    public class SavableTransform : MonoBehaviour
    {
        [SerializeField] bool _position;
        [SerializeField] bool _rotation;

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
            if (_position) GameManager.StoreData.SetKey(name + " pos", transform.position);
            if (_rotation) GameManager.StoreData.SetKey(name + " rot", transform.eulerAngles);
        }

        void Start()
        {
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
        }
    }
}
