using UnityEngine;

namespace Recounter.Tutorial
{
    public class Indicator : MonoBehaviour
    {
        [SerializeField] float _offset;

        static Indicator s_instance;

        void Awake()
        {
            if (s_instance)
                throw new System.Exception("Indicator instance already exists!");

            s_instance = this;

            gameObject.SetActive(false);
        }

        public static void Indicate(Vector3 target)
        {
            s_instance.ShowAt(target);
        }

        public static void Hide()
        {
            s_instance.gameObject.SetActive(false);
        }

        void ShowAt(Vector3 target)
        {
            transform.position = target + Vector3.up * _offset;
            gameObject.SetActive(true);
        }

        void Update()
        {
            transform.Rotate(Time.deltaTime * 90 * Vector3.up);
            transform.Translate(Vector3.up * Mathf.Cos(Time.time * Mathf.PI) * Time.deltaTime);
        }
    }
}
