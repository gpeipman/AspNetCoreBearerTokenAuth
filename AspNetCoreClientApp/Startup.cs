using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AspNetCoreClientApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsDevelopment())
            //{
            //    // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
            //    builder.AddUserSecrets<Startup>();
            //}
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configOptions = new AppConfig();
            Configuration.Bind(configOptions);

            services.Configure<AppConfig>(Configuration);

            services.AddDistributedSqlServerCache(o =>
            {
                o.ConnectionString = Configuration["ConnectionString"];
                o.SchemaName = "dbo";
                o.TableName = "CacheTable";
            });

            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ITokenCacheService, DistributedTokenCacheService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<HttpClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var configOptions = new AppConfig();
            Configuration.Bind(configOptions);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Debug);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = configOptions.AzureAd.ClientId,
                ClientSecret = configOptions.AzureAd.ClientSecret,
                Authority = configOptions.AzureAd.AADInstance + configOptions.AzureAd.TenantId,
                CallbackPath = configOptions.AzureAd.CallbackPath,
                Events = new AuthEvents(configOptions.AzureAd, loggerFactory),
                ResponseType = OpenIdConnectResponseType.CodeIdToken
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
