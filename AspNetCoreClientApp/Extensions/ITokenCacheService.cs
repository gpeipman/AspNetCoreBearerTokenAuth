using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AspNetCoreClientApp
{
    public interface ITokenCacheService
    {
        Task<TokenCache> GetCacheAsync(ClaimsPrincipal claimsPrincipal);
        Task ClearCacheAsync(ClaimsPrincipal claimsPrincipal);

    }
}
