using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public static class SaveGuard
{
    const string Salt = "quantumtunnels";

    static readonly JsonSerializerSettings SerializerSettings = new()
        { NullValueHandling = NullValueHandling.Ignore };

    static string GetShaHash(GameData data)
    {
        using var sha = SHA256.Create();
        var sb = new StringBuilder();

        var temp = data.Protection;

        data.Protection = Salt;

        var json = JsonConvert.SerializeObject(data, SerializerSettings);
        var bytes = Encoding.UTF8.GetBytes(json);

        var hash = sha.ComputeHash(bytes);

        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        // Reset protection to what it was.
        data.Protection = temp;

        return sb.ToString();
    }

    public static void Guard(this GameData data)
    {
        data.Protection = GetShaHash(data);
    }

    public static bool WasAltered(this GameData data)
    {
        var hash = data.Protection;
        var correctHash = GetShaHash(data);

        return hash != correctHash;
    }
}