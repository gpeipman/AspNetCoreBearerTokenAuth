using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AspNetCoreClientApp
{
    public class TokenService : ITokenService
    {
        private readonly AzureAdConfig _adOptions;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly ILogger _logger;

        public TokenService(
            IOptions<AppConfig> options,
            ITokenCacheService tokenCacheService,
            ILogger<TokenService> logger)
        {
            _adOptions = options?.Value?.AzureAd;
            _tokenCacheService = tokenCacheService;
            _logger = logger;
        }

        public async Task<string> GetBearerToken(ClaimsPrincipal user)
        {
            return await GetAccessTokenForResourceAsync(_adOptions.GraphResourceId, user).ConfigureAwait(false);
        }

        private async Task<string> GetAccessTokenForResourceAsync(string resource, ClaimsPrincipal user)
        {
            var userId = user.GetObjectIdentifierValue();
            var issuerValue = user.GetIssuerValue();
            var userName = user.Identity?.Name;

            try
            {
                var authContext = await CreateAuthenticationContext(user)
                    .ConfigureAwait(false);
                var result = await authContext.AcquireTokenSilentAsync(
                    resource,
                    new ClientCredential(_adOptions.ClientId, _adOptions.ClientSecret),
                    new UserIdentifier(userId, UserIdentifierType.UniqueId))
                    .ConfigureAwait(false);

                return result.AccessToken;
            }
            catch (AdalException ex)
            {
                throw new AuthenticationException($"AcquireTokenSilentAsync failed for user: {userId}", ex);
            }
        }

        private async Task<AuthenticationContext> CreateAuthenticationContext(ClaimsPrincipal claimsPrincipal)
        {
            return new AuthenticationContext(
                _adOptions.AuthEndpointPrefix,
                await _tokenCacheService.GetCacheAsync(claimsPrincipal)
                .ConfigureAwait(false));
        }

        public async Task<AuthenticationResult> RequestTokenAsync(
            ClaimsPrincipal claimsPrincipal,
            string authorizationCode,
            string redirectUri,
            string resource)
        {
            try
            {
                var userId = claimsPrincipal.GetObjectIdentifierValue();
                var issuerValue = claimsPrincipal.GetIssuerValue();
                var authenticationContext = await CreateAuthenticationContext(claimsPrincipal)
                    .ConfigureAwait(false);
                var authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(
                    authorizationCode,
                    new Uri(redirectUri),
                    new ClientCredential(_adOptions.ClientId, _adOptions.ClientSecret),
                    resource)
                    .ConfigureAwait(false);

               return authenticationResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ClearCacheAsync(ClaimsPrincipal claimsPrincipal)
        {
            await _tokenCacheService.ClearCacheAsync(claimsPrincipal).ConfigureAwait(false);
        }
    }
}
