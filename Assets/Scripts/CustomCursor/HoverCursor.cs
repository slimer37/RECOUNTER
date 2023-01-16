using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Recounter.Cursors
{
    public class HoverCursor : MonoBehaviour
    {
        [SerializeField] CursorSet _cursorSet;

        GameObject _lastHover;

        bool _cursorActive;

        void Update()
        {
            var module = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            var result = module.GetLastRaycastResult(0).gameObject;

            if (_lastHover == result) return;

            _lastHover = result;

            if (!result)
            {
                Clear();
                return;
            }

            OnNewHoverGained(result);
        }

        void OnNewHoverGained(GameObject hover)
        {
            var target = hover.GetComponentInParent<Selectable>();
            if (!target)
            {
                Clear();
                return;
            }

            Use(target);
        }

        void Use(Selectable target)
        {
            _cursorSet.UseAppropriateCursor(target);
            _cursorActive = true;
        }

        void Clear()
        {
            if (!_cursorActive) return;

            Cursor.SetCursor(null, Vector2.one, CursorMode.Auto);
            _cursorActive = false;
        }
    }
}
