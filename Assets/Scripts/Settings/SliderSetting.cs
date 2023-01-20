using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Recounter.Settings
{
    [RequireComponent(typeof(Slider))]
    public class SliderSetting : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPrefResetter
    {
        [SerializeField] string key;
        [SerializeField] float defaultValue;
        [SerializeField] bool isInt;

        [Header("Optional")]
        [SerializeField] TextMeshProUGUI display;
        [SerializeField] string format = "0.0";

        Slider slider;

        public void ResetPref()
        {
            slider.value = defaultValue;
            UpdatePref();
        }

        void Awake()
        {
            slider = GetComponent<Slider>();

            slider.value = isInt
                ? PlayerPrefs.GetInt(key, (int)defaultValue)
                : PlayerPrefs.GetFloat(key, defaultValue);

            if (!display) return;

            display.text = slider.value.ToString(format);
            slider.onValueChanged.AddListener(v => display.text = v.ToString(format));
        }

        void UpdatePref()
        {
            var value = slider.value;
            if (isInt)
                PrefManager.SetInt(key, (int)value);
            else
                PrefManager.SetFloat(key, value);
        }

        public void OnPointerUp(PointerEventData eventData) => UpdatePref();

        public void OnPointerDown(PointerEventData eventData) { }
    }
}
