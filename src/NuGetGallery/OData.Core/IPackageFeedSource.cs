// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGetGallery.OData.Core
{
    /// <summary>
    /// Placeholder data-access abstraction for the bespoke net10 OData v1/v2 feeds. The real
    /// gallery data access (<c>IReadOnlyEntityRepository&lt;Package&gt;</c>, EF6-backed
    /// <c>EntitiesContext</c>, <c>SearchAdaptor</c>/<c>SearchHijacker</c>) is not wired up yet
    /// -- see <c>OData.Core\README.md</c> for why. Controllers depend only on this interface so
    /// a real implementation can be substituted later without touching the feed/serialization
    /// code.
    /// </summary>
    public interface IPackageFeedSource
    {
        /// <summary>All available versions of a package id, or empty if the id is unknown.</summary>
        IReadOnlyList<V2FeedPackage> FindPackagesById(string id);

        /// <summary>A single package version, or null if not found.</summary>
        V2FeedPackage FindPackage(string id, string version);
    }

    /// <summary>
    /// Placeholder <see cref="IPackageFeedSource"/> backed by an in-memory sample so the
    /// bespoke controllers below have something concrete to serialize and can be exercised
    /// end-to-end (see this task's test_report artifact). NOT the real gallery package
    /// catalog.
    /// </summary>
    public class InMemorySamplePackageFeedSource : IPackageFeedSource
    {
        private readonly List<V2FeedPackage> _packages = new List<V2FeedPackage>
        {
            new V2FeedPackage
            {
                Id = "Sample.Package",
                Version = "1.0.0",
                NormalizedVersion = "1.0.0",
                Authors = "NuGet Gallery",
                Created = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Description = "Sample package used to prove the bespoke net10 OData v1/v2 feed wire format.",
                DownloadCount = 1,
                VersionDownloadCount = 1,
                IsLatestVersion = true,
                IsAbsoluteLatestVersion = true,
                IsPrerelease = false,
                LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Published = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PackageHash = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                PackageHashAlgorithm = "SHA512",
                PackageSize = 1024,
                RequireLicenseAcceptance = false,
                Tags = "sample",
                Title = "Sample Package",
            },
            new V2FeedPackage
            {
                Id = "Sample.Package",
                Version = "0.9.0",
                NormalizedVersion = "0.9.0",
                Authors = "NuGet Gallery",
                Created = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Description = "Earlier sample version.",
                DownloadCount = 1,
                VersionDownloadCount = 0,
                IsLatestVersion = false,
                IsAbsoluteLatestVersion = false,
                IsPrerelease = false,
                LastUpdated = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Published = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                PackageHash = "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                PackageHashAlgorithm = "SHA512",
                PackageSize = 900,
                RequireLicenseAcceptance = false,
                Tags = "sample",
                Title = "Sample Package",
            },
        };

        public IReadOnlyList<V2FeedPackage> FindPackagesById(string id)
        {
            return _packages
                .Where(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public V2FeedPackage FindPackage(string id, string version)
        {
            return _packages.FirstOrDefault(p =>
                string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Version, version, StringComparison.OrdinalIgnoreCase));
        }
    }
}
