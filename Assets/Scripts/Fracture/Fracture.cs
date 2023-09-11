using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Pool;

namespace Recounter.Fracture
{
    public class Fracture : MonoBehaviour
    {
        [SerializeField] string _id;
        [SerializeField] float _fractureVelocity;
        [SerializeField] GameObject _fracturedPrefab;
        [SerializeField] EventReference _fractureSfx;

        static Dictionary<string, ObjectPool<FracturedObject>> Pools;

        bool _alreadyFractured;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Pools = new();
        }

        void OnValidate()
        {
            if (!_fracturedPrefab) return;
            if (!_fracturedPrefab.TryGetComponent<FracturedObject>(out _))
                Debug.LogError("Fractured prefab has no FracturedObject component.");
        }

        void Awake()
        {
            if (!Pools.ContainsKey(_id))
                Pools[_id] = new ObjectPool<FracturedObject>(
                    () => Instantiate(_fracturedPrefab).GetComponent<FracturedObject>(),
                    instance => instance.gameObject.SetActive(true));
        }

        void OnCollisionEnter(Collision other)
        {
            if (_alreadyFractured) return;
            
            if (other.relativeVelocity.magnitude > _fractureVelocity)
            {
                SpawnFractured(other.GetContact(0).point, -other.relativeVelocity);
            }
        }

        void SpawnFractured(Vector3 contact, Vector3 velocity)
        {
            _alreadyFractured = true;
            
            var clone = Pools[_id].Get();
            clone.transform.SetPositionAndRotation(transform.position, transform.rotation);
            clone.Explode(Pools[_id].Release, contact, velocity);
            RuntimeManager.PlayOneShotAttached(_fractureSfx, gameObject);
            Destroy(gameObject);
        }
    }
}