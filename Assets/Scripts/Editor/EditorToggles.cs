using UnityEditor;

namespace Recounter.Editor
{
    public static class EditorToggles
    {
        const string RPCKey = "Enable RPC";
        const string RPCMenuName = "Tools/Enable Rich Presence";

        static EditorToggle RPCEnabled;

        [InitializeOnLoadMethod]
        static void CheckmarkMenuItem()
        {
            RPCEnabled = new EditorToggle(RPCKey, RPCMenuName);
        }
        
        [MenuItem(RPCMenuName)] static void RPCToggle() => RPCEnabled.Toggle();
    }
}
