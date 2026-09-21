// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NuGetGallery.AsyncFileUpload;
using NuGetGallery.Configuration;
using NuGetGallery.Cookies;
using NuGetGallery.Middleware;
using NuGetGallery.OData.Core;

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

            // ----------------------------------------------------------------------------------
            // Cross-cutting Web.config settings, ported to appsettings.json-driven Core services
            // (see src/NuGetGallery/appsettings.json and Configuration/CrossCuttingOptions.cs).
            // ----------------------------------------------------------------------------------

            var dataProtectionOptions = builder.Configuration
                .GetSection(GalleryDataProtectionOptions.SectionName)
                .Get<GalleryDataProtectionOptions>() ?? new GalleryDataProtectionOptions();

            // Replaces the legacy <machineKey configProtectionProvider="GalleryMachineKeyConfigurationProvider">
            // Web.config section (App_Start\GalleryMachineKeyConfigurationProvider.cs). By default (matching the
            // shipped Web.config's "Gallery.EnableMachineKeyConfiguration=false"), each instance manages its own
            // ephemeral Data Protection key ring -- ASP.NET Core's normal default. Only when explicitly
            // configured with a shared key ring directory do multiple instances share keys (needed for cookies
            // to remain valid across instances/slot-swaps, the original problem the machine key configuration
            // solved; see https://github.com/NuGet/Engineering/issues/1329).
            var dataProtectionBuilder = builder.Services
                .AddDataProtection()
                .SetApplicationName(dataProtectionOptions.ApplicationName);
            if (dataProtectionOptions.EnableKeyPersistence && !string.IsNullOrWhiteSpace(dataProtectionOptions.KeyRingDirectory))
            {
                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionOptions.KeyRingDirectory));
            }

            // Replaces the legacy <httpModules>/<modules> "ApplicationInsightsWebTracking" entries
            // (Microsoft.ApplicationInsights.Web.ApplicationInsightsHttpModule, from the Microsoft.AI.Web
            // package) with the ASP.NET Core-native Application Insights SDK. Reads "ApplicationInsights"
            // config the same way the legacy "Gallery.AppInsightsInstrumentationKey" appSetting did, just under
            // the standard ASP.NET Core Application Insights config section instead of Gallery's own.
            builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

            // Replaces NuGetGallery.Modules.CookieComplianceHttpModule and
            // NuGetGallery.AsyncFileUpload.AsyncFileUploadModule (both formerly registered as IHttpModules in
            // Web.config's <httpModules>/<modules> sections); see Middleware/CookieComplianceMiddleware.cs and
            // AsyncFileUpload/AsyncFileUploadProgressMiddleware.cs for the ASP.NET Core middleware equivalents.
            builder.Services.AddSingleton<ICookieComplianceService, NullCookieComplianceService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IUploadProgressCacheService, MemoryUploadProgressCacheService>();

            // Note on <sessionState mode="Off"/>: ASP.NET Core does not add session middleware to the pipeline
            // unless app.UseSession() is called, so "off" requires no action -- see GallerySessionOptions' doc comment.
            // The section/option is bound here only so a future deployment can flip it on without more code.
            var sessionOptions = builder.Configuration.GetSection(GallerySessionOptions.SectionName).Get<GallerySessionOptions>()
                ?? new GallerySessionOptions();
            if (sessionOptions.Enabled)
            {
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSession();
            }

            // Autofac.Extensions.DependencyInjection is the sole DI integration for this app,
            // replacing the legacy Autofac.Mvc5/.Owin/.WebApi2 packages used under classic OWIN.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                // Application composition root: registrations will be ported here from
                // App_Start\AutofacConfig.cs / DefaultDependenciesModule.cs as features land.
            });

            // Bespoke net10 OData v1/v2 feed controllers (see OData.Core\README.md for why
            // this is hand-rolled rather than built on Microsoft.AspNetCore.OData).
            builder.Services.AddSingleton<IPackageFeedSource, InMemorySamplePackageFeedSource>();

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

            if (sessionOptions.Enabled)
            {
                app.UseSession();
            }

            // Must run early, before any downstream code that might rely on the cookie-compliance decision or
            // on upload progress tracking -- matching where the legacy IHttpModules hooked into the classic
            // System.Web pipeline (BeginRequest/PostAuthenticateRequest).
            app.UseMiddleware<CookieComplianceMiddleware>();
            app.UseMiddleware<AsyncFileUploadProgressMiddleware>();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

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

