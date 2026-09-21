// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// ASP.NET Core-native replacement for <c>NuGetGallery.ICacheService</c>
    /// (src/NuGetGallery.Services/Authentication/AsyncFileUpload/*), scoped down to just the upload-progress
    /// use case this project currently ports (the legacy interface also covered generic cache get/set, which
    /// belongs with the rest of the caching subsystem port, not this cross-cutting-concerns step).
    /// </summary>
    public interface IUploadProgressCacheService
    {
        AsyncFileUploadProgress GetProgress(string key);

        void SetProgress(string key, AsyncFileUploadProgress progress);

        void RemoveProgress(string key);
    }
}
