using System.Linq;
using System.Xml.Linq;
using UnityEditor;

// https://forum.unity.com/threads/vs-22-throws-cs8032-after-updating-to-tech-stream-2022-2-0f1.1372701/
public class CS8032WarningFixer : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        var document = XDocument.Parse(content);
        document.Root.Descendants()
            .Where(x => x.Name.LocalName == "Analyzer")
            .Where(x => x.Attribute("Include").Value.Contains("Unity.SourceGenerators"))
            .Remove();
        return document.Declaration + System.Environment.NewLine + document.Root;
    }
}