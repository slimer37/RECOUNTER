using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace slimer37.Serialization
{
    public static class SaveGuard
    {
        const string Salt = "quantumtunnels";

        static readonly JsonSerializerSettings s_serializerSettings = new()
        { NullValueHandling = NullValueHandling.Ignore };

        public static string GetShaHash(SaveData data)
        {
            using var sha = SHA256.Create();
            var sb = new StringBuilder();

            var temp = data.protection;

            data.protection = Salt;

            var json = JsonConvert.SerializeObject(data, s_serializerSettings);
            var bytes = Encoding.UTF8.GetBytes(json);

            var hash = sha.ComputeHash(bytes);

            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            data.protection = temp;

            return sb.ToString();
        }

        public static bool WasAltered(this SaveData data)
            => data.protection != GetShaHash(data);
    }
}