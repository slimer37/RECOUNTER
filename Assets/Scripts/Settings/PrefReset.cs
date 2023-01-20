using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Settings
{
    public class PrefReset : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] GameObject _settingGroup;

        IPrefResetter[] resetters;

        void Awake()
        {
            _button.onClick.AddListener(ResetPrefs);
            resetters = _settingGroup.GetComponentsInChildren<IPrefResetter>();
        }

        void ResetPrefs()
        {
            foreach (var resetter in resetters)
            {
                resetter.ResetPref();
            }
        }
    }
}
