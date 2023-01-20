using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Settings
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSetting : MonoBehaviour
    {
        [SerializeField] string key;
        [SerializeField] bool defaultValue;

        void Awake()
        {
            var toggle = GetComponent<Toggle>();
            toggle.isOn = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
            toggle.onValueChanged.AddListener(OnToggle);
        }

        void OnToggle(bool value)
        {
            PrefManager.SetInt(key, value ? 1 : 0);
        }
    }
}
