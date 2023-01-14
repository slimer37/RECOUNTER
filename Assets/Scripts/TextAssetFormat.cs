using NaughtyAttributes;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Recounter.UI
{
    public class TextAssetFormat : MonoBehaviour
    {
        [Serializable]
        struct RegexReplacement
        {
            public string regex;
            public string replacement;
        }

        [SerializeField] TextAsset _asset;
        [SerializeField] TMP_Text _text;
        [SerializeField] RegexOptions _options;
        [SerializeField] RegexReplacement[] _replacements;

        [Button("Test Regex")]
        void Test() => print(FormatTextAsset());

        void Awake()
        {
            _text.text = FormatTextAsset();

            _text.ForceMeshUpdate();
        }

        string FormatTextAsset()
        {
            var formatted = _asset.text;

            foreach (var r in _replacements)
            {
                formatted = Regex.Replace(formatted, r.regex, r.replacement, _options);
            }

            return formatted;
        }
    }
}