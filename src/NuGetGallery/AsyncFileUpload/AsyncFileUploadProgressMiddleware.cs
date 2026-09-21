// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// ASP.NET Core middleware replacement for the legacy <c>NuGetGallery.AsyncFileUpload.AsyncFileUploadModule</c>
    /// IHttpModule (src/NuGetGallery.Services/Authentication/AsyncFileUpload/AsyncFileUploadModule.cs), which
    /// let the gallery's upload UI (Scripts/gallery/async-file-upload.js) poll a progress endpoint while a
    /// large package upload POST was still streaming in.
    ///
    /// The legacy module relied on IIS-classic-pipeline-only APIs (<c>HttpRequest.GetBufferedInputStream</c>,
    /// <c>HttpRequest.ReadEntityBodyMode</c>) to peek at bytes already received by IIS without disturbing the
    /// request's normal downstream consumption. Kestrel/ASP.NET Core has no equivalent "peek at what IIS has
    /// buffered so far" API, because there is no IIS buffering step to peek at -- the request body is just a
    /// <see cref="Stream"/> that whichever downstream code reads it (e.g. eventual model binding in a ported
    /// controller) reads directly and incrementally as bytes arrive from the client.
    ///
    /// This middleware reproduces the same observable behavior -- upload progress, including the current
    /// file's name, visible to a poller before the request finishes -- the Core-native way: it substitutes
    /// <c>HttpContext.Request.Body</c> with a tee-ing <see cref="ProgressTrackingStream"/> that forwards every
    /// byte read by downstream code (unchanged) while also feeding those same bytes to the ported
    /// <see cref="AsyncFileUploadRequestParser"/> and updating <see cref="IUploadProgressCacheService"/>, so
    /// existing polling clients and cache-key conventions keep working unchanged.
    /// </summary>
    public class AsyncFileUploadProgressMiddleware
    {
        private const int MinimumContentLengthToTrack = 4096;

        private readonly RequestDelegate _next;
        private readonly IUploadProgressCacheService _cacheService;

        public AsyncFileUploadProgressMiddleware(RequestDelegate next, IUploadProgressCacheService cacheService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!IsAsyncUploadRequest(context.Request, out string boundary) ||
                context.User?.Identity == null ||
                !context.User.Identity.IsAuthenticated ||
                string.IsNullOrEmpty(context.User.Identity.Name))
            {
                await _next(context);
                return;
            }

            var username = context.User.Identity.Name;
            var uploadTracingKey = UploadTracingKeyHelper.GetUploadTracingKey(context.Request.Headers);
            var uploadKey = username + uploadTracingKey;

            var progress = new AsyncFileUploadProgress(context.Request.ContentLength ?? 0);
            _cacheService.SetProgress(uploadKey, progress);

            var parser = new AsyncFileUploadRequestParser(boundary, System.Text.Encoding.UTF8);
            var originalBody = context.Request.Body;
            var trackingStream = new ProgressTrackingStream(originalBody, (buffer, count) =>
            {
                parser.ParseNext(buffer, count);
                progress.TotalBytesRead += count;
                progress.FileName = parser.CurrentFileName;
                _cacheService.SetProgress(uploadKey, progress);
            });

            context.Request.Body = trackingStream;
            try
            {
                await _next(context);
            }
            finally
            {
                context.Request.Body = originalBody;
                _cacheService.RemoveProgress(uploadKey);
            }
        }

        private static bool IsAsyncUploadRequest(HttpRequest request, out string boundary)
        {
            boundary = null;

            if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var contentType = request.ContentType;
            if (contentType == null || !contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var boundaryIndex = contentType.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase);
            if (boundaryIndex < 0)
            {
                return false;
            }

            if (!request.ContentLength.HasValue || request.ContentLength.Value < MinimumContentLengthToTrack)
            {
                return false;
            }

            boundary = "--" + contentType.Substring(boundaryIndex + "boundary=".Length).Trim('"');
            return true;
        }

        /// <summary>
        /// Wraps the request body stream, invoking <paramref name="onRead"/> with each chunk as it is read by
        /// downstream code, while otherwise behaving exactly like the wrapped stream. This is what lets progress
        /// tracking observe bytes "as they stream in" without buffering the request or disturbing whatever
        /// downstream component (e.g. MVC model binding) actually needs to consume the body.
        /// </summary>
        private sealed class ProgressTrackingStream : Stream
        {
            private readonly Stream _inner;
            private readonly Action<byte[], int> _onRead;

            public ProgressTrackingStream(Stream inner, Action<byte[], int> onRead)
            {
                _inner = inner;
                _onRead = onRead;
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _inner.Length;

            public override long Position
            {
                get => _inner.Position;
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var read = _inner.Read(buffer, offset, count);
                if (read > 0)
                {
                    Report(buffer, offset, read);
                }

                return read;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
                if (read > 0)
                {
                    Report(buffer, offset, read);
                }

                return read;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var read = await _inner.ReadAsync(buffer, cancellationToken);
                if (read > 0)
                {
                    // The tracking parser works over byte[] buffers; copy the read segment out rather than
                    // requiring every caller to use the array-based overload.
                    var array = buffer.Slice(0, read).ToArray();
                    Report(array, 0, read);
                }

                return read;
            }

            private void Report(byte[] buffer, int offset, int count)
            {
                if (offset == 0)
                {
                    _onRead(buffer, count);
                }
                else
                {
                    var slice = new byte[count];
                    Buffer.BlockCopy(buffer, offset, slice, 0, count);
                    _onRead(slice, count);
                }
            }

            public override void Flush() => _inner.Flush();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
