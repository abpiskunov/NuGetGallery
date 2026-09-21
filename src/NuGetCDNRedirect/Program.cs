// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace NuGet.Services.CDNRedirect
{
    /// <summary>
    /// Minimal ASP.NET Core (net10.0) hosting shell for NuGetCDNRedirect, replacing the legacy
    /// System.Web/OWIN Global.asax + MvcApplication startup. This is a placeholder pipeline
    /// only: the real status page/controllers (see the excluded legacy files under this project)
    /// have not been ported yet and are expected to land in a follow-up feature-port step.
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
                // Application composition root: registrations will be added here as
                // features are ported from the legacy Autofac module configuration.
            });

            var app = builder.Build();

            // Placeholder pipeline: proves the new SDK-style/net10.0 host builds and serves
            // requests. Real routes (e.g. the Status page) are ported in a later step.
            app.MapGet("/", () => Results.Text("NuGetCDNRedirect (net10 scaffold placeholder)"));

            app.Run();
        }
    }
}
