using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class Page : MonoBehaviour
    {
        [SerializeField, Tooltip("Enabled/Disabled when page is opened/closed")]
        Behaviour[] _linked;

        Canvas _canvas;
        GraphicRaycaster _raycaster;

        PageGroup _pageGroup;

        bool _isShowing;

        void Awake()
        {
            gameObject.TryGetComponent(out _canvas);
            gameObject.TryGetComponent(out _raycaster);

            _pageGroup = GetComponentInParent<PageGroup>();

            Show(false);
        }

        public void Close()
        {
            if (!_isShowing) return;

            Show(false);
        }

        public void Open()
        {
            if (_isShowing) return;

            _pageGroup.CloseOtherPages(this);
            Show(true);
        }

        void Show(bool show)
        {
            if (_canvas)
            {
                _canvas.enabled = show;

                if (_raycaster)
                    _raycaster.enabled = show;
            }
            else
            {
                gameObject.SetActive(show);
            }

            foreach (var c in _linked)
            {
                c.enabled = show;
            }

            _isShowing = show;
        }
    }
}