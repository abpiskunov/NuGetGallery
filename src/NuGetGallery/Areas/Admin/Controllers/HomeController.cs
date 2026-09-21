// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Rebuilt for ASP.NET Core (net10.0). See AdminControllerBase.cs for the D2 (Dynamic Data)
// decision and the current auth-wiring caveat.
//
// Narrowed from the legacy version: IContentService/IGalleryConfigurationService (content-cache
// clearing, config-driven nav flags) are not ported yet, so "Clear Content Cache" and the
// config-gated nav flags (Lucene/Validation) are dropped for now -- the nav always shows those
// links so the page is at least navigable once their controllers land. The
// AdminPanelDatabaseAccessEnabled-gated "Database Admin" link is intentionally omitted entirely,
// per the D2 decision to drop the Dynamic Data admin surface rather than rebuild it.

using Microsoft.AspNetCore.Mvc;
using NuGetGallery.Areas.Admin.ViewModels;

namespace NuGetGallery.Areas.Admin.Controllers
{
    public class HomeController : AdminControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new HomeViewModel(
                showDatabaseAdmin: false,
                showLuceneAdmin: true,
                showValidation: true);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Throw()
        {
            throw new System.Exception("KA BOOM!");
        }
    }
}
