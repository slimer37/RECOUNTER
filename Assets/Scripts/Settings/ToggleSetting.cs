using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Settings
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSetting : MonoBehaviour, IPrefResetter
    {
        [SerializeField] string key;
        [SerializeField] bool defaultValue;

        Toggle toggle;

        void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.isOn = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) > 0;
            toggle.onValueChanged.AddListener(OnToggle);
        }

        public void ResetPref() => toggle.isOn = defaultValue;

        void OnToggle(bool value) => PrefManager.SetInt(key, value ? 1 : 0);
    }
}
