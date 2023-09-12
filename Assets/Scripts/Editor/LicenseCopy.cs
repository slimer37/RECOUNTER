using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Recounter.Editor
{
    public static class LicenseCopy
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuildFile)
        {
            File.Copy(Path.Combine(Application.dataPath, "LICENSE.txt"),
                Path.Combine(Path.GetDirectoryName(pathToBuildFile), "LICENSE.txt"),
                true);
        }
    }
}