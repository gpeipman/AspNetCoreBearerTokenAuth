using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AspNetCoreClientApp
{
    public class DistributedTokenCacheService : ITokenCacheService
    {
        private IHttpContextAccessor _contextAccessor;
        private IDataProtectionProvider _dataProtectionProvider;
        private IDistributedCache _distributedCache;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private TokenCache _cache = null;

        public DistributedTokenCacheService(
            IDistributedCache distributedCache,
            IHttpContextAccessor contextAccessor,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _distributedCache = distributedCache;
            _contextAccessor = contextAccessor;
            _dataProtectionProvider = dataProtectionProvider;

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(this.GetType().FullName);
        }

        public Task<TokenCache> GetCacheAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (_cache == null)
            {
                _cache = new DistributedTokenCache(claimsPrincipal, _distributedCache, _loggerFactory, _dataProtectionProvider);
            }

            return Task.FromResult(_cache);
        }

        public virtual async Task ClearCacheAsync(ClaimsPrincipal claimsPrincipal)
        {
            var cache = await GetCacheAsync(claimsPrincipal);
            cache.Clear();
        }
    }
}
