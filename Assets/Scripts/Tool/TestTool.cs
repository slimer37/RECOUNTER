using FMODUnity;
using UnityEngine;

namespace Recounter
{
    public class TestTool : Tool<Transform>
    {
        [SerializeField] LineRenderer _line;
        [SerializeField] EventReference _beep;

        void Awake()
        {
            _line.enabled = false;
        }

        protected override void OnPickUp()
        {
            base.OnPickUp();
            _line.enabled = true;
        }

        public override void Release()
        {
            base.Release();
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
