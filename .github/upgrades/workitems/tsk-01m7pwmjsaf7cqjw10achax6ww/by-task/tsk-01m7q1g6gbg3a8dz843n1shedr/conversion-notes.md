# SDK-style conversion status (Phase 1)

Summary
- All 143 projects are SDK-style (<Project Sdk=...>); no Microsoft.CSharp/VisualBasic targets remain.
- No packages.config remain; PackageReference with central versions in Directory.Packages.props.
- NuGetGallery (System.Web, net472) and NuGetCDNRedirect are SDK-style with UseWPP_CopyWebApplication and explicit Content/ResX/Cshtml includes.

Build
- Windows + Visual Studio MSBuild is required for System.Web projects.
- Command: msbuild NuGetGallery.sln /restore /t:Build /p:Configuration=Debug

Notes
- global.json unchanged.
- Tests: dotnet test --no-build

Validation (2026-11-18)
- Scans confirm no packages.config; all projects SDK-style; no per-project PackageReference versions.
- msbuild build of NuGetGallery.sln succeeded.
