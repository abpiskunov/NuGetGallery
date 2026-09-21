// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NuGetGallery
{
    /// <summary>
    /// Minimal ASP.NET Core (net10.0) hosting shell for the NuGetGallery web app, replacing the
    /// legacy System.Web/OWIN startup (App_Start\OwinStartup.cs, App_Start\AppActivator.cs).
    ///
    /// This is a placeholder pipeline only: it proves the SDK-style, net10.0-targeted project
    /// builds and serves requests through the new hosting model. The full application
    /// composition root (App_Start\AutofacConfig.cs / DefaultDependenciesModule.cs), controllers,
    /// OData feeds, and the Dynamic Data admin UI have not been ported yet -- see the legacy
    /// files retained (but excluded from compilation) elsewhere in this project -- and are
    /// expected to be wired in incrementally by follow-on feature-port steps.
    ///
    /// The Areas/Admin MVC surface (rebuilt to replace ASP.NET Dynamic Data, see
    /// Areas/Admin/Controllers/AdminControllerBase.cs for the D2 decision) is now wired up via
    /// AddControllersWithViews()/MapControllerRoute(areaName: "Admin"), but the real Autofac
    /// composition root (EntitiesContext/repositories/config/etc.) is NOT wired yet -- routes
    /// resolve, views render, but any controller that needs a real service (e.g.
    /// SiteAdminsController's IUserService) will fail at request time until that composition
    /// root lands. See this task's test_report artifact for what was and wasn't verified.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Autofac.Extensions.DependencyInjection is the sole DI integration for this app,
            // replacing the legacy Autofac.Mvc5/.Owin/.WebApi2 packages used under classic OWIN.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                // Application composition root: registrations will be ported here from
                // App_Start\AutofacConfig.cs / DefaultDependenciesModule.cs as features land.
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthorization();

            // Placeholder cookie-authentication scheme so [Authorize(Roles = "Admins")] (on
            // AdminControllerBase) produces a well-formed 302 challenge to a not-yet-existing
            // login path instead of crashing with "no DefaultChallengeScheme found". Porting the
            // real authentication story (the gallery's actual login/claims/cookie machinery,
            // machineKey/GalleryMachineKeyConfigurationProvider, etc.) is the sibling "Port
            // Web.config cross-cutting concerns" task's job, not this one's.
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/users/account/signin";
                });

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "admin_area",
                pattern: "Admin/{controller=Home}/{action=Index}/{id?}",
                defaults: new { area = "Admin" });

            // Placeholder pipeline: proves the new SDK-style/net10.0 host builds and serves
            // requests. Real routes/controllers/views are ported in later feature-port steps.
            app.MapGet("/", () => Results.Text("NuGetGallery (net10 scaffold placeholder)"));

            app.Run();
        }
    }
}

