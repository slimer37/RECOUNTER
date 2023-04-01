using Recounter.Service;
using System.Collections;
using UnityEngine;

namespace Recounter
{
    public class Scanner : ProductEntryModule
    {
        [SerializeField] ToneGenerator _tone;
        [SerializeField] Light _light;
        [SerializeField] float _blinkTime;

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
            _light.enabled = true;
            _tone.Beep();
            yield return new WaitForSeconds(_blinkTime);
            _light.enabled = false;
        }
    }
}
