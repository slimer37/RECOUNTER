using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tutorial
{
    public class Indicator : MonoBehaviour
    {
        [SerializeField] float _bounceSpeed;
        [SerializeField] float _offset;
        [SerializeField] Renderer _renderer;

        [Header("Screen Arrow")]
        [SerializeField] Image _uiArrow;
        [SerializeField] float _arrowBounce;
        [SerializeField] float _margin;

        static Indicator s_instance;

        Vector3 _target;

        bool _active;

        Camera _camera;

        void Awake()
        {
            if (s_instance)
                throw new System.Exception("Indicator instance already exists!");

            s_instance = this;

            _camera = Camera.main;

            Clear();
        }

        float GetOffset() => Mathf.Abs(Mathf.Sin(Mathf.PI / 2 * Time.time * _bounceSpeed));

        public static void BeginIndicate(Vector3 target)
        {
            s_instance.Begin(target);
        }

        public static void EndIndicate()
        {
            s_instance.Clear();
        }

        void Begin(Vector3 target)
        {
            _active = true;

            _target = target;

            gameObject.SetActive(true);
        }

        void Clear()
        {
            _active = false;

            gameObject.SetActive(false);

            _uiArrow.enabled = false;
        }

        void Update()
        {
            if (!_active) return;

            transform.Rotate(Time.deltaTime * 90 * Vector3.up);
            transform.position = _target + Vector3.up * GetOffset();

            var screenPos = _camera.WorldToViewportPoint(_target);

            var isVisible = Mathf.Abs(screenPos.x - 0.5f) < 0.5f && Mathf.Abs(screenPos.y - 0.5f) < 0.5f;

            _uiArrow.enabled = !isVisible;

            if (!isVisible)
            {
                if (screenPos.z < 0)
                {
                    screenPos *= -1;
                }

                var arrowPos = screenPos;

                arrowPos = (arrowPos - Vector3.one / 2) * 2;

                var max = Mathf.Max(Mathf.Abs(arrowPos.x), Mathf.Abs(arrowPos.y));

                arrowPos = arrowPos / (max * 2) + Vector3.one / 2;

                var toTarget = screenPos - arrowPos;

                var angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + 90;

                _uiArrow.transform.localEulerAngles = angle * Vector3.forward;

                var screenDimensions = new Vector3(Screen.width, Screen.height);

                var center = screenDimensions / 2;

                arrowPos = Vector3.Scale(arrowPos, screenDimensions);

                var extent = arrowPos - center;

                arrowPos = center + extent * (1 - _margin);

                _uiArrow.transform.position = arrowPos + _uiArrow.transform.up * GetOffset() * _arrowBounce;
            }
        }
    }
}
