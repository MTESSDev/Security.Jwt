using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using Jwks.Manager.Jwks;

namespace Jwks.Manager
{

    /// <summary>
    /// This points to a JSON file in the format:
    /// {
    ///  "Modulus": "",
    ///  "Exponent": "",
    ///  "P": "",
    ///  "Q": "",
    ///  "DP": "",
    ///  "DQ": "",
    ///  "InverseQ": "",
    ///  "D": ""
    /// }
    /// And some more details to store
    /// </summary>
    public class SecurityKeyWithPrivate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Parameters { get; set; }
        public string KeyId { get; set; }
        public string Type { get; set; }
        public string Algorithm { get; set; }
        public DateTime CreationDate { get; set; }

        public void SetParameters(SecurityKey key, Algorithm alg)
        {
            Parameters = JsonConvert.SerializeObject(key, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            Type = alg.Kty();
            KeyId = key.KeyId;
            Algorithm = alg;
            CreationDate = DateTime.Now;
        }

        public JsonWebKey GetSecurityKey()
        {
            return JsonConvert.DeserializeObject<JsonWebKey>(Parameters);
        }


        public SigningCredentials GetSigningCredentials()
        {
            return new SigningCredentials(GetSecurityKey(), Algorithm);
        }

        public void SetParameters()
        {
            var jsonWebKey = GetSecurityKey();

            Parameters = JsonConvert.SerializeObject(JwksService.RemovePrivateKey(jsonWebKey), 
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}