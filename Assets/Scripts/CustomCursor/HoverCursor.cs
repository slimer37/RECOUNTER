using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class HoverCursor : MonoBehaviour
{
    [SerializeField] Texture2D _cursor;
    [SerializeField] Vector2 _hotspot;

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
        if (!hover.GetComponentInParent<Selectable>())
        {
            Clear();
            return;
        }

        Use();
    }

    void Use()
    {
        if (_cursorActive) return;

        Cursor.SetCursor(_cursor, _hotspot, CursorMode.Auto);
        _cursorActive = true;
    }

    void Clear()
    {
        if (!_cursorActive) return;

        Cursor.SetCursor(null, Vector2.one, CursorMode.Auto);
        _cursorActive = false;
    }
}
