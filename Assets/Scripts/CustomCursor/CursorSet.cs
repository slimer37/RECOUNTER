using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Cursors
{
    [CreateAssetMenu(menuName = "Cursor Set")]
    public class CursorSet : ScriptableObject
    {
        [Serializable]
        struct CustomCursor
        {
            [SerializeField] Texture2D _cursor;
            [SerializeField] Vector2 _hotspot;

            public void Use() => Cursor.SetCursor(_cursor, _hotspot, CursorMode.Auto);
        }

        [SerializeField] CustomCursor _linkCursor;
        [SerializeField] CustomCursor _textCursor;

        public void UseAppropriateCursor(Selectable hover)
        {
            var cursor = hover switch
            {
                TMP_InputField => _textCursor,
                _ => _linkCursor
            };

            cursor.Use();
        }
    }
}
