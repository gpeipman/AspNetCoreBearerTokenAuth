using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace AspNetCoreClientApp
{
    public class AuthEvents : OpenIdConnectEvents
    {
        private readonly AzureAdConfig _azureAdConfig;
        private readonly ILogger _logger;

        public AuthEvents(AzureAdConfig azureAdConfig, ILoggerFactory loggerFactory)
        {
            _azureAdConfig = azureAdConfig;
            _logger = loggerFactory.CreateLogger<AuthEvents>();
        }

        public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            var principal = context.Ticket.Principal;
            var request = context.HttpContext.Request;
            var currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);

            var tokenService = (ITokenService)context.HttpContext.RequestServices.GetService(typeof(ITokenService));
            try
            {
                await tokenService.RequestTokenAsync(
                    principal,
                    context.ProtocolMessage.Code,
                    currentUri,
                    _azureAdConfig.GraphResourceId)
                    .ConfigureAwait(false);
            }
            catch
            {
                await tokenService.ClearCacheAsync(principal).ConfigureAwait(false);
                throw;
            }
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            //_logger.AuthenticationFailed(context.Exception);
            return Task.FromResult(0);
        }

    }
}