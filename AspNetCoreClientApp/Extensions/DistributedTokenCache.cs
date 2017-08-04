using System;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AspNetCoreClientApp
{
    public class DistributedTokenCache : TokenCache
    {
        private ClaimsPrincipal _claimsPrincipal;
        private ILogger _logger;
        private IDistributedCache _distributedCache;
        private IDataProtector _protector;
        private string _cacheKey;

        public DistributedTokenCache(
            ClaimsPrincipal claimsPrincipal,
            IDistributedCache distributedCache,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
            : base()
        {
            //Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));
            //Guard.ArgumentNotNull(distributedCache, nameof(distributedCache));
            //Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            //Guard.ArgumentNotNull(dataProtectionProvider, nameof(dataProtectionProvider));

            _claimsPrincipal = claimsPrincipal;
            _cacheKey = BuildCacheKey(_claimsPrincipal);
            _distributedCache = distributedCache;
            _logger = loggerFactory.CreateLogger<DistributedTokenCache>();
            _protector = dataProtectionProvider.CreateProtector(typeof(DistributedTokenCache).FullName);
            AfterAccess = AfterAccessNotification;
            LoadFromCache();
        }

        private static string BuildCacheKey(ClaimsPrincipal claimsPrincipal)
        {
            //Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));

            string clientId = claimsPrincipal.FindFirstValue("aud", true);
            return string.Format(
                "UserId:{0}::ClientId:{1}",
                claimsPrincipal.GetObjectIdentifierValue(),
                clientId);
        }

        private void LoadFromCache()
        {
            byte[] cacheData = _distributedCache.Get(_cacheKey);
            if (cacheData != null)
            {
                this.Deserialize(_protector.Unprotect(cacheData));
                //_logger.TokensRetrievedFromStore(_cacheKey);
            }
        }

        /// <summary>
        /// Handles the AfterAccessNotification event, which is triggered right after ADAL accesses the cache.
        /// </summary>
        /// <param name="args">An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheNotificationArgs"/> containing information for this event.</param>
        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                try
                {
                    if (this.Count > 0)
                    {
                        _distributedCache.Set(_cacheKey, _protector.Protect(this.Serialize()));
                        //_logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    }
                    else
                    {
                        _distributedCache.Remove(_cacheKey);
                        //_logger.TokenCacheCleared(_claimsPrincipal.GetObjectIdentifierValue(false) ?? "<none>");
                    }
                    this.HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    //_logger.WriteToCacheFailed(exp);
                    throw;
                }
            }
        }
    }
}