using Newtonsoft.Json;

[System.Serializable, JsonObject(MemberSerialization.OptIn)]
public class GameData
{
    [JsonProperty] public string PlayerName { get; private set; }
    [JsonProperty] public string Protection { get; set; }

    public string AccessPath { get; set; }

    public GameData() { }

    public GameData(string accessPath, string playerName)
    {
        AccessPath = accessPath;
        PlayerName = playerName;
    }
}
