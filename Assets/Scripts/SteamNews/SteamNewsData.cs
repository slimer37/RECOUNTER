using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[Serializable]
public struct SteamNewsData
{
    public static string GetApiCall(string appId, int limit) =>
        $"https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={appId}&count={limit}";

    public AppNews appnews;

    [Serializable]
    public struct AppNews
    {
        public SteamPost[] newsitems;
    }

    [Serializable]
    public struct SteamPost
    {
        public string title;
        public string author;
        public string contents;
        public uint date;

        public DateTime GetDateTime() => UnixTimeStampToDateTime(date);
        public string GetReformattedContents() => ConvertBBTags(contents);
    }

    static DateTime UnixTimeStampToDateTime(uint unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    static string ConvertBBTags(string content)
    {
        var listTag = "[list]\n";
        var endListTag = "[/list]\n";
        var endTagReplace = "</indent>";
        var listTagIndex = content.IndexOf(listTag);
        var indent = 0;

        while (listTagIndex > -1)
        {
            content = content[..listTagIndex] + $"<indent={indent * 20}>" + content[(listTagIndex + listTag.Length)..];
            indent++;

            listTagIndex = content.IndexOf(listTag);

            var endListTagIndex = content.IndexOf(endListTag);

            while (endListTagIndex > -1 && endListTagIndex < listTagIndex)
            {
                content = content[..endListTagIndex] + endTagReplace + content[(endListTagIndex + endListTag.Length)..];
                indent--;
                endListTagIndex = content.IndexOf(endListTag);
                listTagIndex = content.IndexOf(listTag);
            }
        }

        foreach (var key in BBTagConversions.Keys)
        {
            var conversion = BBTagConversions[key];
            content = Regex.Replace(content, key, conversion);
        }

        return content.Trim();
    }

    /* language=regex */
    static readonly Dictionary<string, string> BBTagConversions = new()
        {
            { @"\[([biu])](.*?)\[\/\1]", "<$1>$2</$1>" },
            // Reformat list tags
            { @"\[\/list]", "</indent>" },
            { @"\[\*]", "• " },
            // Other
            { @"\[table].*?\[/table]", "[Can't display tables here.]\n" },
            // Links
            { @"\[url=(.*?)](.*?)\[\/url]", "$2 ($1)" },
            // Remove all leftover tags (including closing tags)
            { @"\[(.+)].*?\[\/\1]", "" }
        };
}
