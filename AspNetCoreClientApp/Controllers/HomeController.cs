using System.Collections.Specialized;
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

        public async Task<IActionResult> Index()
        {
            var token = await _tokenService.GetBearerToken(User);
            var authDelegate = new DelegateAuthenticationProvider(
                                    (requestMessage) =>
                                    {
                                        var authHeader = new AuthenticationHeaderValue("bearer", token);
                                        requestMessage.Headers.Authorization = authHeader;
                                        return Task.FromResult(0);
                                    });

            var client = new GraphServiceClient(authDelegate);
            var me = await client.Me.Request().GetAsync();
            var collection = GraphObjectToCollection(me);

            return View(collection);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }

        private NameValueCollection GraphObjectToCollection(object obj)
        {
            var collection = new NameValueCollection();

            if (obj == null)
            {
                return collection;
            }

            var type = obj.GetType();

            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType != typeof(string))
                {
                    continue;
                }

                var valueObject = prop.GetValue(obj);
                if(valueObject == null)
                {
                    continue;
                }

                var value = valueObject.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                collection.Add(prop.Name, value);
            }

            return collection;
        }
    }
}
