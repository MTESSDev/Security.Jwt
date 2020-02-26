using IdentityServer4.Models;
using IdentityServer4.Stores;
using Jwks.Manager.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Jwks.Manager.IdentityServer4
{
    internal class IdentityServer4KeyStore : IValidationKeysStore, ISigningCredentialStore
    {
        private readonly IJsonWebKeySetService _keyService;
        private readonly IMemoryCache _memoryCache;

        /// <summary>Constructor for IdentityServer4KeyStore.</summary>
        /// <param name="keyService"></param>
        public IdentityServer4KeyStore(IJsonWebKeySetService keyService, IMemoryCache memoryCache)
        {
            this._keyService = keyService;
            _memoryCache = memoryCache;
        }

        /// <summary>Returns the current signing key.</summary>
        /// <returns></returns>
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            if (!_memoryCache.TryGetValue("ASPNET-IS4-CURRENT-KEY", out SigningCredentials credentials))
            {
                credentials = _keyService.GetCurrent();
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromDays(1));

                if (credentials != null)
                    _memoryCache.Set("ASPNET-IS4-CURRENT-KEY", credentials, cacheEntryOptions);
            }

            return Task.FromResult(credentials);
        }

        /// <summary>Returns all the validation keys.</summary>
        /// <returns></returns>
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            if (!_memoryCache.TryGetValue("ASPNET-IS4-VALIDATION-KEY", out IReadOnlyCollection<JsonWebKey> credentials))
            {
                credentials = _keyService.GetLastKeysCredentials(5);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromDays(1));

                if (!credentials.Any())
                {
                    _keyService.Generate();
                    credentials = _keyService.GetLastKeysCredentials(5);
                }
                _memoryCache.Set("ASPNET-IS4-VALIDATION-KEY", credentials, cacheEntryOptions);
            }
            return Task.FromResult(credentials.Select(s => new SecurityKeyInfo()
            {
                Key = s,
                SigningAlgorithm = s.Alg
            }));
        }
    }
}