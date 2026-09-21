// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            // Bespoke net10 OData v1/v2 feed controllers (see OData.Core\README.md for why
            // this is hand-rolled rather than built on Microsoft.AspNetCore.OData).
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IPackageFeedSource, InMemorySamplePackageFeedSource>();

            var app = builder.Build();

            app.MapControllers();

            // Placeholder pipeline: proves the new SDK-style/net10.0 host builds and serves
            // requests. Real routes/controllers/views are ported in later feature-port steps.
            app.MapGet("/", () => Results.Text("NuGetGallery (net10 scaffold placeholder)"));

            app.Run();
        }
    }
}
