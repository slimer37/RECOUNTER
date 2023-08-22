using FMODUnity;
using UnityEngine;

namespace Recounter
{
    public class TestTool : Tool<Transform>
    {
        [SerializeField] LineRenderer _line;
        [SerializeField] EventReference _beep;

        protected override void Awake()
        {
            base.Awake();
            _line.enabled = false;
        }

        protected override void OnEquip()
        {
            _line.enabled = true;
        }

        protected override void OnUnequip()
        {
            _line.enabled = false;
        }

        protected override void UseOn(Transform obj)
        {
            RuntimeManager.PlayOneShot(_beep);

            print(obj.name);

            _line.SetPosition(0, _line.transform.position);
            _line.SetPosition(1, obj.position);
        }
    }
}
