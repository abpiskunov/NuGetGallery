// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NuGetGallery.Cookies
{
    /// <summary>
    /// ASP.NET Core-native replacement for <c>NuGetGallery.Cookies.ICookieComplianceService</c>
    /// (src/NuGetGallery.Core/Cookies/ICookieComplianceService.cs), which is defined in terms of
    /// <c>System.Web.HttpRequestBase</c> and is therefore not usable from this project (which no longer
    /// references System.Web). The contract is otherwise unchanged: it answers whether the gallery is
    /// allowed to write each cookie category for the given request, used to comply with EU privacy law
    /// (e.g. GDPR/ePrivacy consent requirements).
    /// </summary>
    public interface ICookieComplianceService
    {
        Task<bool> CanWriteAnalyticsCookiesAsync(HttpRequest request);

        Task<bool> CanWriteSocialMediaCookiesAsync(HttpRequest request);

        Task<bool> CanWriteAdvertisingCookiesAsync(HttpRequest request);
    }

    /// <summary>
    /// Default, no-op cookie compliance service used until a real (e.g. consent-management-platform-backed)
    /// implementation is ported/registered. Mirrors the legacy
    /// <c>NuGetGallery.Cookies.NullCookieComplianceService</c>'s conservative "deny by default" behavior.
    /// </summary>
    public class NullCookieComplianceService : ICookieComplianceService
    {
        public Task<bool> CanWriteAnalyticsCookiesAsync(HttpRequest request) => Task.FromResult(false);

        public Task<bool> CanWriteSocialMediaCookiesAsync(HttpRequest request) => Task.FromResult(false);

        public Task<bool> CanWriteAdvertisingCookiesAsync(HttpRequest request) => Task.FromResult(false);
    }
}
