// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace NuGetGallery.AsyncFileUpload
{
    /// <summary>
    /// Trimmed, local copy of <c>NuGetGallery.RegexEx</c> (src/NuGetGallery.Core/Extensions/RegexEx.cs), which
    /// is not referenceable here because NuGetGallery.Core does not (yet) target net10.0. Kept in this
    /// namespace/folder rather than reused so it is obviously scoped to the async-upload parser below; once
    /// NuGetGallery.Core is ported this local copy should be deleted in favor of a real project reference.
    /// </summary>
    internal static class RegexEx
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        public static Match MatchWithTimeoutOrNull(string input, string pattern, RegexOptions options)
        {
            try
            {
                return Regex.Match(input, pattern, options, Timeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return null;
            }
        }
    }
}
