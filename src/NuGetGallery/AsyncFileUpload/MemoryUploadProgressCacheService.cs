// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Caching.Memory;

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// <see cref="IUploadProgressCacheService"/> implementation backed by <see cref="IMemoryCache"/>, the
    /// ASP.NET Core-native in-process cache. Replaces the legacy
    /// <c>NuGetGallery.HttpContextCacheService</c>, which stored progress in
    /// <c>System.Web.HttpContext.Cache</c> (also process-local, so this preserves the same
    /// single-instance-only semantics -- progress polling from a different gallery instance than the one
    /// receiving the upload never worked under System.Web either, since IIS's HttpContext.Cache is likewise
    /// per-process).
    /// </summary>
    public class MemoryUploadProgressCacheService : IUploadProgressCacheService
    {
        private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(10);

        private readonly IMemoryCache _cache;

        public MemoryUploadProgressCacheService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public AsyncFileUploadProgress GetProgress(string key)
        {
            _cache.TryGetValue(key, out AsyncFileUploadProgress progress);
            return progress;
        }

        public void SetProgress(string key, AsyncFileUploadProgress progress)
        {
            _cache.Set(key, progress, SlidingExpiration);
        }

        public void RemoveProgress(string key)
        {
            _cache.Remove(key);
        }
    }
}
