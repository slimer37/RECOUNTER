using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SteamNews : MonoBehaviour
{
    [SerializeField] string appId;
    [SerializeField] int limit;
    [SerializeField] string postFormat = @"{0}\n{1}\n{2}";
    [SerializeField] string postSeparator = @"\n";

    [SerializeField] TextMeshProUGUI content;

    string unescPostFormat;
    string unescPostSeparator;

    void Awake()
    {
        unescPostFormat = Regex.Unescape(postFormat);
        unescPostSeparator = Regex.Unescape(postSeparator);
    }

    IEnumerator Start()
    {
        content.text = "Loading...";

        var request = UnityWebRequest.Get(SteamNewsData.GetApiCall(appId, limit));

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            content.text = $"<color=red>{request.error}</color>";
            throw new Exception(request.error);
        }

        var json = request.downloadHandler.text;
        var data = JsonUtility.FromJson<SteamNewsData>(json);

        yield return PopulateUI(data.appnews.newsitems);
    }

    IEnumerator PopulateUI(SteamNewsData.SteamPost[] posts)
    {
        var formattedPosts = new string[posts.Length];

        for (var i = 0; i < posts.Length; i++)
        {
            var p = posts[i];

            formattedPosts[i] =
                string.Format(unescPostFormat,
                p.title,
                p.GetDateTime().ToString("g"),
                p.GetReformattedContents());

            yield return null;
        }

        content.text = string.Join(unescPostSeparator, formattedPosts);
    }
}
