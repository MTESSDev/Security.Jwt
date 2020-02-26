using Jwks.Manager.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jwks.Manager.Jwks
{
    public class InMemoryStore : IJsonWebKeyStore
    {
        private readonly IOptions<JwksOptions> _options;
        private List<SecurityKeyWithPrivate> _store;
        private SecurityKeyWithPrivate _current;

        public InMemoryStore(IOptions<JwksOptions> options)
        {
            _options = options;
            _store = new List<SecurityKeyWithPrivate>();
        }

        public void Save(SecurityKeyWithPrivate securityParamteres)
        {
            _store.Add(securityParamteres);
            _current = securityParamteres;
        }

        public bool NeedsUpdate()
        {
            if (_current == null)
                return true;

            return _current.CreationDate.AddDays(_options.Value.DaysUntilExpire) < DateTime.UtcNow.Date;
        }

        public SecurityKeyWithPrivate GetCurrentKey()
        {
            return _current;
        }

        public IReadOnlyCollection<SecurityKeyWithPrivate> Get(int quantity = 5)
        {
            return
                _store
                    .OrderByDescending(s => s.CreationDate)
                    .Take(quantity).ToList().AsReadOnly();
        }

        public void Clear()
        {
            _store.Clear();
        }
    }
}