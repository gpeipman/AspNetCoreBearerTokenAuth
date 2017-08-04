using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace AspNetCoreClientApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ITokenService _tokenService;

        public HomeController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            var token = await _tokenService.GetTokenForWebApiAsync(User);

            var authDelegate = new DelegateAuthenticationProvider(
                                    (requestMessage) =>
                                    {
                                        var authHeader = new AuthenticationHeaderValue("bearer", token);
                                        requestMessage.Headers.Authorization = authHeader;
                                        return Task.FromResult(0);
                                });

            var client = new GraphServiceClient(authDelegate);
            var blah = await client.Users.Request().GetAsync();

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
