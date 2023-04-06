using TMPro;
using UnityEngine;

namespace Recounter
{
    public class TipText : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] Canvas _canvas;
        [SerializeField] string _defaultText;

        void Awake()
        {
            foreach (var tip in _canvas.GetComponentsInChildren<Tip>())
            {
                tip.TipText = this;
            }

            ResetTip();
        }

        public void ResetTip()
        {
            _text.text = _defaultText;
        }

        public void ShowTip(string text)
        {
            _text.text = text;
        }
    }
}
