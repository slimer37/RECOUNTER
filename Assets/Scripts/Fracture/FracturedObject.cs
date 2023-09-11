using System;
using System.Collections;
using UnityEngine;

namespace Recounter.Fracture
{
    public class FracturedObject : MonoBehaviour
    {
        [SerializeField] Rigidbody[] _rigidbodies;
        [SerializeField] ParticleSystem _particles;
        [SerializeField] float _lifetime = 5;
        [SerializeField] float _explosionForce = 150;

        Vector3[] _originalRbPositions;
        Quaternion[] _originalRbRotations;

        void OnValidate()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.layer != LayerMask.NameToLayer("Fracture Piece"))
                {
                    Debug.LogWarning($"All children of {name} must be on the Fracture Piece layer.");
                    return;
                }
            }
        }

        void Reset() => _rigidbodies = GetComponentsInChildren<Rigidbody>();

        void Awake()
        {
            _originalRbPositions = new Vector3[_rigidbodies.Length];
            _originalRbRotations = new Quaternion[_rigidbodies.Length];
            for (var i = 0; i < _rigidbodies.Length; i++)
            {
                _originalRbPositions[i] = _rigidbodies[i].transform.localPosition;
                _originalRbRotations[i] = _rigidbodies[i].transform.localRotation;
            }
        }

        internal void Explode(Action<FracturedObject> disableCallback, Vector3 contact, Vector3 velocity)
        {
            foreach (var rb in _rigidbodies)
            {
                rb.AddForce(velocity, ForceMode.VelocityChange);
                rb.AddExplosionForce(_explosionForce, contact, 0);
            }
        
            if (_particles) _particles.Play();
            StartCoroutine(DelayedOut(disableCallback));
        }

        IEnumerator DelayedOut(Action<FracturedObject> disableCallback)
        {
            yield return new WaitForSeconds(_lifetime);
            for (var i = 0; i < _rigidbodies.Length; i++)
            {
                var rbT = _rigidbodies[i].transform;
                rbT.localPosition = _originalRbPositions[i];
                rbT.localRotation = _originalRbRotations[i];
                _rigidbodies[i].velocity = Vector3.zero;
            }
        
            gameObject.SetActive(false);
            disableCallback(this);
        }
    }
}
