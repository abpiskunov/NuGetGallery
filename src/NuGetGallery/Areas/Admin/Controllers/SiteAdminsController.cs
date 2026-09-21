// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Rebuilt for ASP.NET Core (net10.0). See AdminControllerBase.cs for the D2 (Dynamic Data)
// decision and the current auth-wiring caveat.
//
// NOT CURRENTLY COMPILED: this file is intentionally excluded from NuGetGallery.csproj (see the
// commented-out <Compile Include> there). IUserService/UserService live in
// NuGetGallery.Services/UserManagement/*.cs, which is entirely removed from that project's
// netstandard2.1 leg (NuGetGallery.Services.csproj: <Compile Remove="UserManagement\*.cs" />
// under the netstandard2.1-only ItemGroup) -- and netstandard2.1 is the only leg a net10.0 app
// can reference today, since NuGetGallery.Services has no net10.0 TargetFramework yet. That is
// tracked by the sibling "Shared class libraries: add/verify net10 target" stage (still
// in-progress as of this writing). Once that stage adds net10.0 (or otherwise makes
// UserManagement available outside net472), re-enable this controller and its view/Compile items
// -- the code below should not need further changes, it is otherwise a straight mechanical port.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NuGetGallery.Areas.Admin.ViewModels;

namespace NuGetGallery.Areas.Admin.Controllers
{
    public class SiteAdminsController : AdminControllerBase
    {
        private readonly IUserService _userService;

        public SiteAdminsController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(
                new SiteAdminsViewModel
                {
                    AdminUsernames = _userService.GetSiteAdmins().Select(u => u.Username)
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AddAdmin(string username)
        {
            return SetIsAdministrator(username, true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RemoveAdmin(string username)
        {
            return SetIsAdministrator(username, false);
        }

        private async Task<IActionResult> SetIsAdministrator(string username, bool isAdmin)
        {
            var user = _userService.FindByUsername(username);
            if (user == null)
            {
                TempData["ErrorMessage"] = $"User '{username}' does not exist!";
            }
            else
            {
                await _userService.SetIsAdministrator(user, isAdmin);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
