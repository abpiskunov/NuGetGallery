// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace NuGetGallery.OData.Core
{
    /// <summary>
    /// Hand-written atom+xml writer reproducing the wire shape the legacy
    /// <c>System.Web.Http.OData</c> formatter + <c>NuGetEntityTypeSerializer</c> +
    /// <c>V1FeedPackageAnnotationStrategy</c>/<c>V2FeedPackageAnnotationStrategy</c> produced
    /// (see <c>..\OData\Serializers\*</c>), without depending on any OData library (per the
    /// D3 spike finding that <c>Microsoft.AspNetCore.OData</c> cannot serve this wire format).
    /// Covers only the element shapes exercised by this session's bespoke controllers -- see
    /// <c>OData.Core\README.md</c> for what is and is not covered.
    /// </summary>
    public static class ODataAtomFeedWriter
    {
        private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace DataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace Metadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private const string Scheme = "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme";

        public static XDocument WriteEntry(string siteRoot, string feedName, string entityTypeName, V2FeedPackage package)
        {
            var entry = BuildEntry(siteRoot, feedName, entityTypeName, package);
            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), entry);
        }

        public static XDocument WriteFeed(string siteRoot, string feedName, string entityTypeName, string selfHref, string title, IEnumerable<V2FeedPackage> packages)
        {
            var feed = new XElement(
                Atom + "feed",
                new XAttribute(XNamespace.Xmlns + "d", DataServices.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "m", Metadata.NamespaceName),
                new XAttribute("xmlns", Atom.NamespaceName),
                new XElement(Atom + "title", new XAttribute("type", "text"), title),
                new XElement(Atom + "id", $"{siteRoot}/{selfHref}"),
                new XElement(Atom + "updated", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
                new XElement(Atom + "link", new XAttribute("rel", "self"), new XAttribute("title", "Packages"), new XAttribute("href", selfHref)));

            foreach (var package in packages)
            {
                feed.Add(BuildEntry(siteRoot, feedName, entityTypeName, package));
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), feed);
        }

        private static XElement BuildEntry(string siteRoot, string feedName, string entityTypeName, V2FeedPackage package)
        {
            var packageIdentityQuery = $"(Id='{package.Id}',Version='{package.NormalizedVersion ?? package.Version}')";
            var selfLink = $"{feedName}/Packages{packageIdentityQuery}";
            var downloadUrl = $"{siteRoot}/{feedName}/package/{package.Id}/{package.NormalizedVersion ?? package.Version}";

            var entry = new XElement(
                Atom + "entry",
                new XAttribute(XNamespace.Xmlns + "d", DataServices.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "m", Metadata.NamespaceName),
                new XAttribute("xmlns", Atom.NamespaceName),
                new XElement(Atom + "id", $"{siteRoot}/{selfLink}"),
                new XElement(
                    Atom + "category",
                    new XAttribute("term", $"NuGetGallery.OData.{entityTypeName}"),
                    new XAttribute("scheme", Scheme)),
                new XElement(Atom + "title", new XAttribute("type", "text"), package.Title ?? package.Id),
                new XElement(Atom + "updated", package.LastUpdated.ToString("o", CultureInfo.InvariantCulture)));

            if (!string.IsNullOrEmpty(package.Authors))
            {
                entry.Add(new XElement(Atom + "author", new XElement(Atom + "name", package.Authors)));
            }

            if (!string.IsNullOrEmpty(package.Summary))
            {
                entry.Add(new XElement(Atom + "summary", new XAttribute("type", "text"), package.Summary));
            }

            entry.Add(new XElement(
                Atom + "link",
                new XAttribute("rel", "edit"),
                new XAttribute("title", entityTypeName),
                new XAttribute("href", selfLink)));

            entry.Add(new XElement(
                Atom + "content",
                new XAttribute("type", "application/zip"),
                new XAttribute("src", downloadUrl)));

            entry.Add(BuildProperties(package));

            return entry;
        }

        private static XElement BuildProperties(V2FeedPackage package)
        {
            XElement Prop(string name, object value, string edmType = null)
            {
                var element = new XElement(DataServices + name, value?.ToString());
                if (value == null)
                {
                    element.SetAttributeValue(Metadata + "null", "true");
                }
                else if (edmType != null)
                {
                    element.SetAttributeValue(Metadata + "type", edmType);
                }

                return element;
            }

            return new XElement(
                Metadata + "properties",
                Prop("Version", package.Version),
                Prop("NormalizedVersion", package.NormalizedVersion),
                Prop("Authors", package.Authors),
                Prop("Copyright", package.Copyright),
                Prop("Created", package.Created.ToString("o", CultureInfo.InvariantCulture), "Edm.DateTime"),
                Prop("Dependencies", package.Dependencies),
                Prop("Description", package.Description),
                Prop("DownloadCount", package.DownloadCount, "Edm.Int64"),
                Prop("GalleryDetailsUrl", package.GalleryDetailsUrl),
                Prop("IconUrl", package.IconUrl),
                Prop("IsLatestVersion", XmlConvert.ToString(package.IsLatestVersion), "Edm.Boolean"),
                Prop("IsAbsoluteLatestVersion", XmlConvert.ToString(package.IsAbsoluteLatestVersion), "Edm.Boolean"),
                Prop("IsPrerelease", XmlConvert.ToString(package.IsPrerelease), "Edm.Boolean"),
                Prop("Language", package.Language),
                Prop("LastUpdated", package.LastUpdated.ToString("o", CultureInfo.InvariantCulture), "Edm.DateTime"),
                Prop("Published", package.Published.ToString("o", CultureInfo.InvariantCulture), "Edm.DateTime"),
                Prop("PackageHash", package.PackageHash),
                Prop("PackageHashAlgorithm", package.PackageHashAlgorithm),
                Prop("PackageSize", package.PackageSize, "Edm.Int64"),
                Prop("ProjectUrl", package.ProjectUrl),
                Prop("ReportAbuseUrl", package.ReportAbuseUrl),
                Prop("ReleaseNotes", package.ReleaseNotes),
                Prop("RequireLicenseAcceptance", XmlConvert.ToString(package.RequireLicenseAcceptance), "Edm.Boolean"),
                Prop("Summary", package.Summary),
                Prop("Tags", package.Tags),
                Prop("Title", package.Title),
                Prop("VersionDownloadCount", package.VersionDownloadCount, "Edm.Int64"),
                Prop("MinClientVersion", package.MinClientVersion),
                Prop("LastEdited", package.LastEdited?.ToString("o", CultureInfo.InvariantCulture), "Edm.DateTime"),
                Prop("LicenseUrl", package.LicenseUrl),
                Prop("LicenseNames", package.LicenseNames),
                Prop("LicenseReportUrl", package.LicenseReportUrl));
        }
    }
}
