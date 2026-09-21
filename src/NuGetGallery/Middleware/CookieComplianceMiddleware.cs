// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuGetGallery.Cookies;

namespace NuGetGallery.Middleware
{
    /// <summary>
    /// ASP.NET Core middleware replacement for the legacy <c>NuGetGallery.Modules.CookieComplianceHttpModule</c>
    /// IHttpModule (src/NuGetGallery/Modules/CookieComplianceHttpModule.cs), which hooked
    /// <c>HttpApplication.AddOnBeginRequestAsync</c> to stash a "can we write analytics cookies" decision on
    /// <c>HttpContext.Items</c> before any downstream code ran, so views/controllers could consult it without
    /// each having to call the (potentially remote/slow) compliance service themselves.
    ///
    /// This middleware performs the same job: it must run early in the pipeline (registered right after the
    /// hosting/exception-handling middleware in Program.cs, before anything that might read the item), and it
    /// stores the result under the same key (<see cref="ServicesConstants.CookieComplianceCanWriteAnalyticsCookies"/>-
    /// equivalent, see <see cref="CanWriteAnalyticsCookiesItemKey"/>) that the legacy module used, so ported
    /// controllers/views can read <c>HttpContext.Items[CookieComplianceMiddleware.CanWriteAnalyticsCookiesItemKey]</c>
    /// exactly as they did under System.Web.
    /// </summary>
    public class CookieComplianceMiddleware
    {
        /// <summary>
        /// Matches <c>NuGetGallery.ServicesConstants.CookieComplianceCanWriteAnalyticsCookies</c>
        /// (src/NuGetGallery.Services/ServicesConstants.cs) so ported call sites need no key rename.
        /// </summary>
        public const string CanWriteAnalyticsCookiesItemKey = "CanWriteAnalyticsCookies";

        private readonly RequestDelegate _next;
        private readonly ICookieComplianceService _cookieComplianceService;
        private readonly ILogger<CookieComplianceMiddleware> _logger;

        public CookieComplianceMiddleware(
            RequestDelegate next,
            ICookieComplianceService cookieComplianceService,
            ILogger<CookieComplianceMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _cookieComplianceService = cookieComplianceService ?? throw new ArgumentNullException(nameof(cookieComplianceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var canWriteAnalyticsCookies = false;
            try
            {
                canWriteAnalyticsCookies = await _cookieComplianceService.CanWriteAnalyticsCookiesAsync(context.Request);
            }
            catch (Exception exception)
            {
                _logger.LogError(0, exception, "Cookie compliance check failed in the middleware: {MiddlewareName}", nameof(CookieComplianceMiddleware));
            }

            context.Items[CanWriteAnalyticsCookiesItemKey] = canWriteAnalyticsCookies;

            await _next(context);
        }
    }
}
