using UnityEditor;

public static class RichPresenceToggle
{
    public const string PrefKey = "Enable RPC";

    const string MenuName = "Tools/Enable Rich Presence";

    static bool enabled;

    [InitializeOnLoadMethod]
    static void CheckmarkMenuItem()
    {
        enabled = EditorPrefs.GetBool(PrefKey, true);

        EditorApplication.delayCall += () => SetEnabled(enabled);
    }

    [MenuItem(MenuName)]
    private static void ToggleAction()
    {
        SetEnabled(!enabled);
    }

    public static void SetEnabled(bool enabled)
    {
        Menu.SetChecked(MenuName, enabled);
        EditorPrefs.SetBool(PrefKey, enabled);

        RichPresenceToggle.enabled = enabled;
    }
}
