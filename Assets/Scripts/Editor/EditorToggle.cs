using UnityEditor;

namespace Recounter.Editor
{
    public class EditorToggle
    {
        public readonly string PrefKey;
        public readonly string MenuName;

        public bool Enabled { get; private set; }

        public EditorToggle(string prefKey, string menuName, bool defaultValue = true)
        {
            PrefKey = prefKey;
            MenuName = menuName;

            Enabled = EditorPrefs.GetBool(PrefKey, defaultValue);
            
            EditorApplication.delayCall += () => SetEnabled(Enabled);
        }

        void SetEnabled(bool enabled)
        {
            Menu.SetChecked(MenuName, Enabled);
            EditorPrefs.SetBool(PrefKey, Enabled);
            Enabled = enabled;
        }
        
        public void Toggle() => SetEnabled(!Enabled);
    }
}