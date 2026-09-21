// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This file has been rebuilt for ASP.NET Core (net10.0) as part of the Dynamic Data admin UI
// rebuild task. The pre-existing System.Web.Mvc version of this file (and of the rest of
// Areas/Admin) is retained only in git history now; there is no parallel legacy copy kept in
// the tree, matching the "replace in place, port incrementally" convention established by the
// project-shell scaffold step.
//
// D2 decision applied here (see this work item's spike artifacts): the generic ASP.NET Dynamic
// Data admin/database-browser surface (Areas/Admin/DynamicData, mounted at Admin/Database) is
// DROPPED, not rebuilt -- it was 100% stock scaffolding, disabled by default
// (Gallery.AdminPanelDatabaseAccessEnabled=false), with no bespoke behavior to preserve. Only the
// hand-built Admin MVC controllers (this file's hierarchy) are being ported.
//
// Scope note: authentication/authorization middleware (cookie auth, the real "Admins" role
// claim, antiforgery wiring, etc.) has NOT been wired into Program.cs yet -- that is the
// "Port Web.config cross-cutting concerns" sibling task. The [Authorize(Roles = "Admins")]
// attribute below is applied for parity with the legacy [UIAuthorize(Roles="Admins")], but until
// an authentication scheme is configured it will simply challenge/deny every request (no
// authenticated principal exists). This is intentional and documented, not silently dropped.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NuGetGallery.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admins")]
    public abstract class AdminControllerBase : Controller
    {
        // The legacy SearchForPackages/CreatePackageSearchResult helpers (used by
        // UpdateListedController, LockPackageController, PackageOwnershipController, etc.) are
        // intentionally not ported yet -- none of the controllers ported so far need them, and
        // porting them pulls in IPackageService, NuGetVersion parsing, and the Url.User(...)
        // route-helper extension (Users/Profiles isn't ported to ASP.NET Core yet either). Add
        // them back here when the first package-search-dependent Admin controller is ported.
    }
}

