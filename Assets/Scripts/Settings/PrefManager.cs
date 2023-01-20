using System;
using UnityEngine;

namespace Recounter.Settings
{
    public static class PrefManager
    {
        public static event Action<string, string> OnStringPrefChanged;
        public static event Action<string, int> OnIntPrefChanged;
        public static event Action<string, float> OnFloatPrefChanged;

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            OnStringPrefChanged?.Invoke(key, value);
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            OnIntPrefChanged?.Invoke(key, value);
        }
        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            OnFloatPrefChanged?.Invoke(key, value);
        }

        public static void SetCallbacks(IPrefListener listener)
        {
            OnStringPrefChanged += listener.OnStringPrefChanged;
            OnIntPrefChanged += listener.OnIntPrefChanged;
            OnFloatPrefChanged += listener.OnFloatPrefChanged;
        }

        public static void RemoveCallbacks(IPrefListener listener)
        {
            OnStringPrefChanged -= listener.OnStringPrefChanged;
            OnIntPrefChanged -= listener.OnIntPrefChanged;
            OnFloatPrefChanged -= listener.OnFloatPrefChanged;
        }
    }

    public interface IPrefListener
    {
        public void OnStringPrefChanged(string key, string value);
        public void OnIntPrefChanged(string key, int value);
        public void OnFloatPrefChanged(string key, float value);
    }
}