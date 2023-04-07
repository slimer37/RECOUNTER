using UnityEngine;

namespace Recounter
{
    public class DrawerRail : MonoBehaviour
    {
        [SerializeField] Transform _rail;
        [SerializeField] Transform _midrail;
        [SerializeField] Transform _slide;

        Vector3 _basePosition;

        void Awake()
        {
            _basePosition = _midrail.transform.position;
        }

        void Update()
        {
            _midrail.transform.position = _basePosition + Vector3.Project(_slide.transform.position - _rail.transform.position, _rail.up) / 2;
        }
    }
}
