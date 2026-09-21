// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NuGetGallery.OData.Facts
{
    /// <summary>
    /// Integration tests for the bespoke net10 OData v1/v2 feed controllers
    /// (<c>src\NuGetGallery\Controllers.Core\ODataV{1,2}FeedController.cs</c>), run against a
    /// real in-memory instance of the NuGetGallery net10.0 host via
    /// <see cref="WebApplicationFactory{TEntryPoint}"/>.
    ///
    /// These assert the same kind of wire-format contract the legacy
    /// tests\NuGetGallery.Facts\Controllers\ODataV{1,2}FeedControllerFacts.cs asserted against
    /// the old System.Web.Http.OData controllers (service document, $metadata, single-entry
    /// GET, and FindPackagesById() feed shapes -- atom+xml with m:properties/d:* elements),
    /// adapted to the placeholder InMemorySamplePackageFeedSource data this bespoke
    /// implementation currently serves (see OData.Core/README.md).
    /// </summary>
    public class ODataFeedControllerFacts : IClassFixture<WebApplicationFactory<Program>>
    {
        private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace DataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace Metadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private static readonly XNamespace App = "http://www.w3.org/2007/app";

        private readonly WebApplicationFactory<Program> _factory;

        public ODataFeedControllerFacts(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task ServiceDocument_ReturnsPackagesWorkspace(string apiVersion)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var xml = XDocument.Parse(await response.Content.ReadAsStringAsync());
            var collection = xml.Root.Element(App + "workspace").Element(App + "collection");
            Assert.Equal("Packages", (string)collection.Attribute("href"));
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task Metadata_ReturnsEdmxDocument(string apiVersion)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}/$metadata");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var xml = XDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Edmx", xml.Root.Name.LocalName);
        }

        [Theory]
        [InlineData("v1", "V1FeedPackage")]
        [InlineData("v2", "V2FeedPackage")]
        public async Task GetSpecificPackage_ReturnsAtomEntryWithExpectedProperties(string apiVersion, string expectedEntityTypeName)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}/Packages(Id='Sample.Package',Version='1.0.0')");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/atom+xml", response.Content.Headers.ContentType.MediaType);

            var entry = XDocument.Parse(await response.Content.ReadAsStringAsync()).Root;
            Assert.Equal(Atom + "entry", entry.Name);

            var category = entry.Element(Atom + "category");
            Assert.Contains(expectedEntityTypeName, (string)category.Attribute("term"));

            var properties = entry.Element(Metadata + "properties");
            Assert.Equal("Sample.Package", (string)properties.Element(DataServices + "Id"));
            Assert.Equal("1.0.0", (string)properties.Element(DataServices + "Version"));
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task GetSpecificPackage_UnknownPackage_ReturnsNotFound(string apiVersion)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}/Packages(Id='Does.Not.Exist',Version='1.0.0')");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task FindPackagesById_ReturnsFeedWithAllVersions(string apiVersion)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}/FindPackagesById()?id='Sample.Package'");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/atom+xml", response.Content.Headers.ContentType.MediaType);

            var feed = XDocument.Parse(await response.Content.ReadAsStringAsync()).Root;
            Assert.Equal(Atom + "feed", feed.Name);

            var entries = feed.Elements(Atom + "entry").ToList();
            Assert.Equal(2, entries.Count);
            Assert.All(entries, entry =>
                Assert.Equal(
                    "Sample.Package",
                    (string)entry.Element(Metadata + "properties").Element(DataServices + "Id")));
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task FindPackagesById_UnknownId_ReturnsEmptyFeed(string apiVersion)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/{apiVersion}/FindPackagesById()?id='Does.Not.Exist'");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var feed = XDocument.Parse(await response.Content.ReadAsStringAsync()).Root;
            Assert.Equal(Atom + "feed", feed.Name);
            Assert.Empty(feed.Elements(Atom + "entry"));
        }
    }
}
