// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NuGetGallery.OData.Core;

namespace NuGetGallery.Controllers.Core
{
    /// <summary>
    /// Bespoke net10 reimplementation of the legacy V2 OData package feed
    /// (<c>src\NuGetGallery\Controllers\ODataV2FeedController.cs</c>, excluded from this
    /// project's build), covering only the route shapes and behavior listed as implemented in
    /// <c>OData.Core\README.md</c>. Registered explicitly in <c>Program.cs</c> under
    /// <c>/api/v2</c> (NOT via <c>Microsoft.AspNetCore.OData</c> -- see that README for why).
    /// </summary>
    [ApiController]
    public class ODataV2FeedController : ControllerBase
    {
        private const string FeedName = "api/v2";
        private const string EntityTypeName = "V2FeedPackage";

        private readonly IPackageFeedSource _feedSource;

        public ODataV2FeedController(IPackageFeedSource feedSource)
        {
            _feedSource = feedSource;
        }

        private string SiteRoot => $"{Request.Scheme}://{Request.Host}";

        // GET /api/v2/
        [HttpGet("api/v2")]
        public IActionResult ServiceDocument()
        {
            var xml =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
<service xmlns=""http://www.w3.org/2007/app"" xmlns:atom=""http://www.w3.org/2005/Atom"" xml:base=""{SiteRoot}/api/v2/"">
  <workspace>
    <atom:title>Default</atom:title>
    <collection href=""Packages"">
      <atom:title>Packages</atom:title>
    </collection>
  </workspace>
</service>";
            return Content(xml, "application/xml");
        }

        // GET /api/v2/$metadata
        [HttpGet("api/v2/$metadata")]
        public IActionResult Metadata()
        {
            return Content(ODataMetadataDocument.V2, "application/xml");
        }

        // GET /api/v2/Packages(Id='{id}',Version='{version}')
        [HttpGet("api/v2/Packages(Id='{id}',Version='{version}')")]
        public IActionResult GetSpecificPackage(string id, string version)
        {
            var package = _feedSource.FindPackage(id, version);
            if (package == null)
            {
                return NotFound();
            }

            var document = ODataAtomFeedWriter.WriteEntry(SiteRoot, FeedName, EntityTypeName, package);
            return Content(document.ToString(), "application/atom+xml");
        }

        // GET /api/v2/FindPackagesById()?id='{id}'
        [HttpGet("api/v2/FindPackagesById()")]
        public IActionResult FindPackagesById([FromQuery] string id)
        {
            id = TrimODataStringLiteral(id);
            var packages = _feedSource.FindPackagesById(id).ToList();
            var document = ODataAtomFeedWriter.WriteFeed(
                SiteRoot,
                FeedName,
                EntityTypeName,
                $"{FeedName}/FindPackagesById()?id='{id}'",
                "FindPackagesById",
                packages);
            return Content(document.ToString(), "application/atom+xml");
        }

        internal static string TrimODataStringLiteral(string value)
        {
            if (value != null && value.Length >= 2 && value.StartsWith("'", System.StringComparison.Ordinal) && value.EndsWith("'", System.StringComparison.Ordinal))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}
