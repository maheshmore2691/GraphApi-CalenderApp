using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Scheduler.WebClient.Helpers;
using Scheduler.WebClient.Interfaces;
using Scheduler.WebClient.Models;

namespace Scheduler.WebClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfig config = Configuration.Get<Config>();
            services.AddSingleton(config);

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            //        options =>
            //        {
            //            options.Cookie.Name = "Scheduling.Client";
            //            options.Cookie.SameSite = SameSiteMode.None;
            //        })
            //    .AddOpenIdConnect("aad", "Microsoft Login", options =>
            //    {
            //        options.Authority = "https://login.microsoftonline.com/common";
            //        options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false };
            //        options.ClientId = "";
            //        options.CallbackPath = "/signin-oidc";
            //        options.SaveTokens = true;
            //        options.Scope.Add("https://graph.microsoft.com/Calendars.ReadWrite");

            //        options.Events = new OpenIdConnectEvents()
            //        {
            //            OnRemoteFailure = context =>
            //            {
            //                context.Response.Redirect("/");
            //                context.HandleResponse();
            //                return Task.CompletedTask;
            //            }
            //        };
            //    });

            services.AddControllersWithViews();
            services.AddTransient<IGraphApiClient, GraphApiClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=RegisterTenant}/{id?}");
            });
        }
    }
}
