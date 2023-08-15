using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Recounter.Store.Security
{
    public static class SaveGuard
    {
        const string Salt = "quantumtunnels";

        static readonly JsonSerializerSettings s_serializerSettings = new()
        { NullValueHandling = NullValueHandling.Ignore };

        public static string GetShaHash(StoreData data)
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

        public static bool WasAltered(this StoreData data)
            => data.protection != GetShaHash(data);
    }
}