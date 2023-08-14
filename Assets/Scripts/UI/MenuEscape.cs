using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Recounter.UI
{
    public class MenuEscape : MonoBehaviour
    {
        class MenuStack<T>
        {
            readonly List<T> _values = new();

            public void Push(T item) => _values.Add(item);

            public bool TryPop(out T result)
            {
                result = default;

                if (_values.Count == 0) return false;

                result = _values[^1];

                return true;
            }

            public bool Remove(T item) => _values.Remove(item);

            public void Clear() => _values.Clear();
        }

        Canvas _canvas;

        static readonly MenuStack<MenuEscape> _menus = new();

        public event Action Opened;
        public event Action Closed;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            InputLayer.Menu.Exit.performed += _ => EscapeTopMenu();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) => _menus.Clear();

        bool _isOpen;

        static void EscapeTopMenu()
        {
            if (Dialog.Main.IsOpen || !_menus.TryPop(out var menu)) return;

            if (!menu._isOpen)
            {
                EscapeTopMenu();
            }
            else
            {
                menu.Escape();
            }
        }

        void Awake()
        {
            if (!TryGetComponent(out _canvas))
            {
                Debug.LogWarning("No canvas found to use as menu.", this);
            }
        }

        void Update()
        {
            if (_isOpen != _canvas.enabled)
            {
                _isOpen = _canvas.enabled;

                if (_isOpen)
                {
                    _menus.Push(this);
                    Opened?.Invoke();
                }
                else
                {
                    _menus.Remove(this);
                    Closed?.Invoke();
                }
            }
        }

        void Escape()
        {
            _canvas.enabled = false;
        }
    }
}
