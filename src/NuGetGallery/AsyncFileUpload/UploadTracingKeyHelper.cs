// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// Ported from <c>NuGetGallery.Helpers.UploadHelper.GetUploadTracingKey</c>
    /// (src/NuGetGallery.Services/Helpers/UploadHelper.cs), adapted from <c>NameValueCollection</c> headers to
    /// ASP.NET Core's <see cref="IHeaderDictionary"/>. Behavior is unchanged: an old/other client that does not
    /// send the tracing header, or sends a malformed one, falls back to <see cref="Guid.Empty"/> rather than
    /// failing the request -- simultaneous uploads from such a client may then share/clobber one progress
    /// tracking key, exactly as under the legacy implementation.
    /// </summary>
    public static class UploadTracingKeyHelper
    {
        /// <summary>
        /// Matches <c>NuGetGallery.CoreConstants.UploadTracingKeyHeaderName</c>
        /// (src/NuGetGallery.Core/CoreConstants.cs) so existing clients need no change.
        /// </summary>
        public const string UploadTracingKeyHeaderName = "upload-id";

        public static string GetUploadTracingKey(IHeaderDictionary headers)
        {
            if (headers == null)
            {
                return Guid.Empty.ToString();
            }

            string uploadTracingKey;
            try
            {
                uploadTracingKey = headers[UploadTracingKeyHeaderName];
                uploadTracingKey = Guid.Parse(uploadTracingKey).ToString();
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is OverflowException)
            {
                // An upload tracing key was not found, or was not a well-formed GUID.
                // Simultaneous UI uploads might have strange behaviour.
                // Note that we might have this case if an old client sends to a new server.
                uploadTracingKey = Guid.Empty.ToString();
            }

            return uploadTracingKey;
        }
    }
}
