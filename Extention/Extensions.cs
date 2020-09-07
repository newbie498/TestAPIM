using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerTest.Extention
{
    public static class Extensions
    {
        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(section).Bind(model);
            return model;
        }
        public static IServiceCollection AddAzureAuthentication(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            var logger = services.BuildServiceProvider().GetService<ILogger<IServiceCollection>>();
            var hostingEnvironment = services.BuildServiceProvider().GetService<IHostingEnvironment>();
           // services.AddTransient<ITicketStore, DistributedCacheTicketStore>();

            CookieAuthenticationSetting cookieAuthenticationSettings = ConfigureOptions(services, configuration);
            //services.Configure<DistributedCacheTicketStoreOptions>(o =>
            //{
            //    o.TimeoutMinutes = TimeSpan.FromMilliseconds(cookieAuthenticationSettings.CookieExpiration + Constants.SESSIONTIMEOUTBUFFER).TotalMinutes;
            //});
            services.AddAuthentication(o =>
            {
                o.DefaultSignInScheme = cookieAuthenticationSettings.CookieName;
                o.DefaultScheme = cookieAuthenticationSettings.CookieName;
                o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCustomCookie(cookieAuthenticationSettings.CookieName, options =>
            {
                options.LoginPath = cookieAuthenticationSettings.LoginPath;
                options.LogoutPath = cookieAuthenticationSettings.LogoutPath;
                options.Cookie.Name = cookieAuthenticationSettings.CookieName;
                options.ExpireTimeSpan = TimeSpan.FromMilliseconds(cookieAuthenticationSettings.ExpireTimeSpan + Constants.SESSIONTIMEOUTBUFFER);
                options.SlidingExpiration = cookieAuthenticationSettings.SlidingExpiration;
                options.Cookie.Expiration = TimeSpan.FromMilliseconds(cookieAuthenticationSettings.CookieExpiration + Constants.SESSIONTIMEOUTBUFFER);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnSignedIn = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnSigningOut = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnValidatePrincipal = context =>
                    {
                        ClaimsPrincipal principal = context.Principal;
                        if (context.HttpContext.Request.ContentLength > 0)
                        {
                            context.ReplacePrincipal(principal);
                            context.ShouldRenew = true;
                        }
                        return Task.CompletedTask;
                    }
                };
                if (cookieAuthenticationSettings.SessionStore)
                {
                    options.SessionStore = services.BuildServiceProvider().GetService<ITicketStore>();
                }
                options.Events.OnRedirectToLogin = (context) =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            })
            .AddAzure(configuration, logger, hostingEnvironment, "Azure");

            return services;
        }

        private static CookieAuthenticationSetting ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookieAuthenticationSetting>(configuration.GetSection("CookieAuthenticationSetting"));
            return configuration.GetOptions<CookieAuthenticationSetting>("CookieAuthenticationSetting");
        }
    }
}
