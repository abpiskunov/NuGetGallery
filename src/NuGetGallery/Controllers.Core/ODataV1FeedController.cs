// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NuGetGallery.OData.Core;

namespace NuGetGallery.Controllers.Core
{
    /// <summary>
    /// Bespoke net10 reimplementation of the legacy V1 OData package feed
    /// (<c>src\NuGetGallery\Controllers\ODataV1FeedController.cs</c>, excluded from this
    /// project's build). See <c>ODataV2FeedController</c> and <c>OData.Core\README.md</c> for
    /// the approach and its limits; V1 additionally omits every SemVer2-only property the V1
    /// wire format never had (this controller reuses the V2 feed source/writer's fields that
    /// overlap V1FeedPackage -- see <c>OData.Core\README.md</c> for the scope this covers).
    /// </summary>
    [ApiController]
    public class ODataV1FeedController : ControllerBase
    {
        private const string FeedName = "api/v1";
        private const string EntityTypeName = "V1FeedPackage";

        private readonly IPackageFeedSource _feedSource;

        public ODataV1FeedController(IPackageFeedSource feedSource)
        {
            _feedSource = feedSource;
        }

        private string SiteRoot => $"{Request.Scheme}://{Request.Host}";

        // GET /api/v1/
        [HttpGet("api/v1")]
        public IActionResult ServiceDocument()
        {
            var xml =
                $@"<?xml version=""1.0"" encoding=""utf-8""?>
<service xmlns=""http://www.w3.org/2007/app"" xmlns:atom=""http://www.w3.org/2005/Atom"" xml:base=""{SiteRoot}/api/v1/"">
  <workspace>
    <atom:title>Default</atom:title>
    <collection href=""Packages"">
      <atom:title>Packages</atom:title>
    </collection>
  </workspace>
</service>";
            return Content(xml, "application/xml");
        }

        // GET /api/v1/$metadata
        [HttpGet("api/v1/$metadata")]
        public IActionResult Metadata()
        {
            return Content(ODataMetadataDocument.V1, "application/xml");
        }

        // GET /api/v1/Packages(Id='{id}',Version='{version}')
        [HttpGet("api/v1/Packages(Id='{id}',Version='{version}')")]
        public IActionResult GetSpecificPackage(string id, string version)
        {
            var package = _feedSource.FindPackage(id, version);
            if (package == null)
            {
                return NotFound();
            }

            // V1's wire format is a strict subset of V2's (no SemVer2/license-report fields);
            // reusing the V2 writer/POCO here still produces a superset an old client will
            // simply ignore the extra elements of, which is a deliberate scope-reduction for
            // this session -- NOT the byte-for-byte V1 shape. See OData.Core/README.md.
            var document = ODataAtomFeedWriter.WriteEntry(SiteRoot, FeedName, EntityTypeName, package);
            return Content(document.ToString(), "application/atom+xml");
        }

        // GET /api/v1/FindPackagesById()?id='{id}'
        [HttpGet("api/v1/FindPackagesById()")]
        public IActionResult FindPackagesById([FromQuery] string id)
        {
            id = ODataV2FeedController.TrimODataStringLiteral(id);
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
    }
}
