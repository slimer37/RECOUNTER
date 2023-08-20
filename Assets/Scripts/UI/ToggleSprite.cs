using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSprite : MonoBehaviour
    {
        [SerializeField] Graphic _offGraphic;

        void Awake()
        {
            TryGetComponent(out Toggle toggle);

            OnToggle(toggle.isOn);

            toggle.onValueChanged.AddListener(OnToggle);
        }

        void OnToggle(bool on)
        {
            _offGraphic.enabled = !on;
        }
    }
}
