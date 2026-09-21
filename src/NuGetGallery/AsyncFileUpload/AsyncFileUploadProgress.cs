// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// Progress of an in-flight package upload, keyed and polled the same way as the legacy
    /// <c>NuGetGallery.AsyncFileUpload.AsyncFileUploadProgress</c>
    /// (src/NuGetGallery.Services/Authentication/AsyncFileUpload/AsyncFileUploadProgress.cs): the browser's
    /// upload UI (Scripts/gallery/async-file-upload.js) polls a progress endpoint keyed by
    /// username + upload-tracing-id while a POST to the upload action is still being received.
    /// </summary>
    public class AsyncFileUploadProgress
    {
        public AsyncFileUploadProgress(long contentLength)
        {
            ContentLength = contentLength;
        }

        public long ContentLength { get; }

        public long TotalBytesRead { get; set; }

        public long BytesRemaining => ContentLength - TotalBytesRead;

        public string FileName { get; set; }
    }
}
