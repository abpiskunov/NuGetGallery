// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGetGallery.OData.Core
{
    /// <summary>
    /// net10.0 bespoke re-declaration of the legacy <c>NuGetGallery.OData.V2FeedPackage</c>
    /// (<c>src\NuGetGallery\OData\V2FeedPackage.cs</c>), minus the
    /// <c>System.Data.Services.Common</c> attributes. See <c>OData.Core\README.md</c>.
    /// </summary>
    public class V2FeedPackage
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string NormalizedVersion { get; set; }

        public string Authors { get; set; }
        public string Copyright { get; set; }
        public DateTime Created { get; set; }
        public string Dependencies { get; set; }
        public string Description { get; set; }
        public long DownloadCount { get; set; }
        public string GalleryDetailsUrl { get; set; }
        public string IconUrl { get; set; }
        public bool IsLatestVersion { get; set; }
        public bool IsAbsoluteLatestVersion { get; set; }
        public bool IsPrerelease { get; set; }
        public string Language { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Published { get; set; }
        public string PackageHash { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public long PackageSize { get; set; }
        public string ProjectUrl { get; set; }
        public string ReportAbuseUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public long VersionDownloadCount { get; set; }
        public string MinClientVersion { get; set; }
        public DateTime? LastEdited { get; set; }

        // License Report Information
        public string LicenseUrl { get; set; }
        public string LicenseNames { get; set; }
        public string LicenseReportUrl { get; set; }
    }
}
