using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreClientApp
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory, IOptions<AppConfig> options)
        {
            //var httpHandler = new HttpClientLogHandler(
            //        new HttpClientHandler(),
            //        loggerFactory.CreateLogger<HttpClientLogHandler>(),
            //        contextAccessor);

            //_httpClient = new HttpClient(httpHandler);
            _httpClient = new HttpClient(null);

            // Set the BaseAddress of the HttpClient if WebApiUrl is found in configuration.
            var baseAddress = options?.Value?.AzureAd.GraphResourceId;
            if (!string.IsNullOrEmpty(baseAddress))
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}
