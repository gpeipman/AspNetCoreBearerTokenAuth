using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AspNetCoreClientApp
{
    public interface ITokenService
    {
        Task<string> GetBearerToken(ClaimsPrincipal user);

        Task<AuthenticationResult> RequestTokenAsync(
            ClaimsPrincipal claimsPrincipal,
            string authorizationCode,
            string redirectUri,
            string resource);
        Task ClearCacheAsync(ClaimsPrincipal claimPrincipal);

    }
}
