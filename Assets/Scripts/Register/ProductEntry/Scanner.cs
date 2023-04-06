using FMODUnity;
using Recounter.Service;
using System.Collections;
using UnityEngine;

namespace Recounter
{
    public class Scanner : ProductEntryModule
    {
        [SerializeField] Light _light;
        [SerializeField] float _blinkTime;
        [SerializeField] EventReference _beep;

        void Awake()
        {
            _light.enabled = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_light.enabled) return;

            if (other.TryGetComponent(out ProductIdentifier identifier))
            {
                EnterProduct(Library.Products[identifier.id]);

                StopAllCoroutines();
                StartCoroutine(Blink());
            }
        }

        IEnumerator Blink()
        {
            RuntimeManager.PlayOneShotAttached(_beep, gameObject);

            _light.enabled = true;

            yield return new WaitForSeconds(_blinkTime);

            _light.enabled = false;
        }
    }
}
