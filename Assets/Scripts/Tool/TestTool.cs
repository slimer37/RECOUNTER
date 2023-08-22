using UnityEngine;

namespace Recounter
{
    public class TestTool : Tool<Transform>
    {
        [SerializeField] LineRenderer _line;

        protected override void UseOn(Transform obj)
        {
            print(obj.name);

            _line.SetPosition(0, _line.transform.position);
            _line.SetPosition(1, obj.position);
        }
    }
}
