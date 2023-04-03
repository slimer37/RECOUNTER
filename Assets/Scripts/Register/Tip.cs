using UnityEngine;
using UnityEngine.EventSystems;

namespace Recounter
{
    public class Tip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, TextArea] string _tip;

        public TipText TipText { get; set; }

        public string Text => _tip;

        public void OnPointerEnter(PointerEventData eventData) => TipText.ShowTip(_tip);

        public void OnPointerExit(PointerEventData eventData) => TipText.ResetTip();
    }
}
